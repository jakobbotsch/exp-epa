﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace QT
{
    internal class TypeChecker
    {
        private readonly ContextInfo _ctxInfo;
        private readonly IModel _model;

        public TypeChecker(IModel model)
        {
            _model = model;
            _ctxInfo = new ContextInfo(this, model.EmptyCtx());
        }

        public Tm TypeCheck(Def def)
        {
            using (_ctxInfo.Remember())
            {
                foreach (CtxExt ctxExt in def.CtxExts)
                    ExtendContext(ctxExt);

                Ty retTy = TypeCheckType(def.RetTy);
                Tm body = TypeCheckTerm(def.Body, retTy);

                return body;
            }
        }

        private Ty ExtendContext(CtxExt ext)
        {
            Ty ty = TypeCheckType(ext.Type);
            _ctxInfo.Extend(ext.Name.IsDiscard ? null : ext.Name.Name, ty);
            // Put this term into the context because it might have equalities that we need.
            _ctxInfo.AccessLast();
            return ty;
        }

        private Ty TypeCheckType(Expr expr)
        {
            if (!(TypeCheck(expr) is Ty ty))
                throw new Exception($"{expr} is not a type");

            return ty;
        }

        private Tm TypeCheckAnyTerm(Expr expr)
        {
            if (!(TypeCheck(expr) is Tm tm))
                throw new Exception($"{expr} is not a term");

            return tm;
        }

        private Tm TypeCheckTerm(Expr expr, Ty ty)
        {
            Tm tm = TypeCheckAnyTerm(expr);
            if (!_model.TyEq(ty, tm.Ty))
            {
                string msg =
                    $"Unexpected type of term.{Environment.NewLine}" +
                    $"Expected: {ty}{Environment.NewLine}" +
                    $"Actual: {tm.Ty}";
                throw new Exception(msg);
            }

            return tm;
        }

        private CwfNode TypeCheck(Expr expr)
        {
            switch (expr)
            {
                case IdExpr { Id: string id }:
                    if (id == "bool")
                        return _model.Bool(_ctxInfo.Ctx);
                    if (id == "true")
                        return _model.True(_ctxInfo.Ctx);
                    if (id == "false")
                        return _model.False(_ctxInfo.Ctx);

                    return _ctxInfo.AccessVar(id);
                case LetExpr let:
                    using (_ctxInfo.Remember())
                    {
                        Ty ty = TypeCheckType(let.Type);
                        Tm tm = TypeCheckTerm(let.Val, ty);
                        if (!let.Name.IsDiscard)
                            _ctxInfo.Define(let.Name.Name, tm);
                        return TypeCheck(let.Body);
                    }
                case AppExpr { Fun: string fun, Args: var args }:
                    if (fun == "id" && args.Count == 2)
                    {
                        Tm a = TypeCheckAnyTerm(args[0]);
                        Tm b = TypeCheckTerm(args[1], a.Ty);
                        return _model.Id(a, b);
                    }
                    if (fun == "refl" && args.Count == 1)
                    {
                        Tm a = TypeCheckAnyTerm(args[0]);
                        return _model.Refl(a);
                    }
                    if (fun == "dump")
                    {
                        CwfNode result = TypeCheck(args[1]);
                        _model.Dump(((IdExpr)args[0]).Id);
                        return result;
                    }

                    goto default;
                case ElimExpr elim:
                    Tm discTm = TypeCheckAnyTerm(elim.Discriminee);
                    if (_model.TyEq(discTm.Ty, _model.Bool(_ctxInfo.Ctx)))
                        return ElimBool(discTm, elim);

                    goto default;
                default:
                    throw new Exception("Unhandled: " + expr);
            }
        }

        // Given G |- tm : ty, make <id, tm> : G -> G.ty
        private CtxMorph TermBar(Tm tm)
        {
            IdMorph idMorph = _model.IdMorph(tm.Ctx);
            ExtensionMorph ext = _model.Extension(idMorph, tm, tm.Ty);
            return ext;
        }

        // Given f : D -> G and G |- s type, construct
        // wk f : D.s{f} -> G.s as <f . p1(s{f}), p2(s{f})>
        private ExtensionMorph Weaken(CtxMorph morph, Ty ty)
        {
            Debug.Assert(_model.CtxEq(morph.Codomain, ty.Ctx));

            // s{f}
            SubstTy substTy = _model.SubstType(ty, morph);

            // D.s{f}
            ComprehensionCtx ctxFrom = _model.Comprehension(morph.Domain, substTy);

            // p1(s{f}) : D.s{f} -> D
            ProjMorph projCtx = _model.ProjCtx(ctxFrom);
            // D.s{f} |- p2(s{f}) : s{f}
            ProjTm projTm = _model.ProjTm(ctxFrom);

            CompMorph comp = _model.Compose(morph, projCtx);
            return _model.Extension(comp, projTm, ty);
        }

        private Tm ElimBool(Tm discriminee, ElimExpr elim)
        {
            if (elim.IntoExts.Count != 1)
                throw new Exception($"Invalid context extension {string.Join(" ", elim.IntoExts)}");

            if (elim.Cases.Count != 2)
                throw new Exception($"Expected 2 cases for nat elimination");

            if (elim.Cases[0].CaseExts.Count != 0)
                throw new Exception($"{elim.Cases[0]}: expected no context extensions");

            if (elim.Cases[1].CaseExts.Count != 0)
                throw new Exception($"{elim.Cases[1]}: expected no context extensions");

            BoolTy boolInElimCtx = _model.Bool(_ctxInfo.Ctx);
            Ty intoTy;
            using (_ctxInfo.Remember())
            {
                Ty extTy = ExtendContext(elim.IntoExts[0]);
                if (!_model.TyEq(extTy, boolInElimCtx))
                    throw new Exception($"Expected extension by bool, got extension by {extTy}");

                intoTy = TypeCheckType(elim.IntoTy);
            }

            TrueTm @true = _model.True(_ctxInfo.Ctx);
            SubstTy expectedTyTrueCase = _model.SubstType(intoTy, TermBar(@true));
            Tm trueCase = TypeCheckTerm(elim.Cases[0].Body, expectedTyTrueCase);

            Tm @false = _model.False(_ctxInfo.Ctx);
            Ty expectedTyFalseCase = _model.SubstType(intoTy, TermBar(@false));
            Tm falseCase = TypeCheckTerm(elim.Cases[1].Body, expectedTyFalseCase);

            ElimBoolTm elimTm = _model.ElimBool(intoTy, trueCase, falseCase);

            CtxMorph addDisc = TermBar(discriminee);
            Tm substTm = _model.SubstTerm(elimTm, addDisc);
            return substTm;
        }

        private class ContextInfo
        {
            private readonly TypeChecker _tc;

            // tm is either a defined term in context at i - 1, or null if there was an extension.
            private readonly List<(string? name, Tm? tm)> _vars = new List<(string? name, Tm? tm)>();

            public ContextInfo(TypeChecker tc, Ctx initialCtx)
            {
                _tc = tc;
                Ctx = initialCtx;
            }

            public int NumVars => _vars.Count;

            public Ctx Ctx { get; private set; }

            // Extend the context with a term of the specified type.
            public void Extend(string? name, Ty ty)
            {
                Debug.Assert(_tc._model.CtxEq(ty.Ctx, Ctx));
                _vars.Add((name, null));
                Ctx = _tc._model.Comprehension(Ctx, ty);
            }

            // Define a term in the current context.
            public void Define(string name, Tm tm)
            {
                Debug.Assert(_tc._model.CtxEq(tm.Ctx, Ctx));
                _vars.Add((name, tm));
            }

            // Construct a term that accesses the var by the specified name.
            public Tm AccessVar(string name)
            {
                int index = _vars.FindLastIndex(t => t.name == name);
                if (index == -1)
                    throw new ArgumentException($"No variable exists by name {name}", nameof(name));

                return Access(index);
            }

            public Tm AccessLast()
            {
                if (_vars.Count <= 0)
                    throw new InvalidOperationException("No variables have been added to the context");

                return Access(_vars.Count - 1);
            }

            private Tm Access(int index)
            {
                Debug.Assert(Ctx is ComprehensionCtx);
                return Go(_vars.Count - 1, (ComprehensionCtx)Ctx);

                Tm Go(int i, ComprehensionCtx nextCtx)
                {
                    (string? name, Tm? definedTm) = _vars[i];
                    // Make context morphism that gets us from nextCtx to ty's
                    // ctx by projecting out added variable.

                    if (i == index)
                    {
                        if (definedTm != null)
                            return definedTm;

                        ProjMorph ctxProj = _tc._model.ProjCtx(nextCtx);
                        Tm tmProj = _tc._model.ProjTm(nextCtx);
                        return tmProj;
                    }
                    else
                    {
                        if (definedTm != null)
                        {
                            // Skip definition that does not extend context
                            return Go(i - 1, nextCtx);
                        }

                        // BaseCtx should always be a comprehension as
                        // otherwise either we got all the way back to index 0
                        // (taking above branch) or to terms defined in the
                        // empty context (also taking above branch).
                        Debug.Assert(nextCtx.BaseCtx is ComprehensionCtx);
                        ComprehensionCtx prevCtx = (ComprehensionCtx)nextCtx.BaseCtx;
                        Tm tm = Go(i - 1, prevCtx);

                        ProjMorph ctxProj = _tc._model.ProjCtx(nextCtx);
                        Tm tmSubst = _tc._model.SubstTerm(tm, ctxProj);
                        return tmSubst;
                    }
                }
            }

            public ContextSavePoint Remember()
                => new ContextSavePoint(this);

            public struct ContextSavePoint : IDisposable
            {
                private readonly ContextInfo _ctx;
                private readonly int _numVars;
                private readonly Ctx _cwfCtx;
                public ContextSavePoint(ContextInfo ctx)
                {
                    _ctx = ctx;
                    _numVars = ctx._vars.Count;
                    _cwfCtx = ctx.Ctx;
                }

                public void Dispose()
                {
                    Debug.Assert(_ctx._vars.Count >= _numVars);

                    _ctx._vars.RemoveRange(_numVars, _ctx._vars.Count - _numVars);
                    _ctx.Ctx = _cwfCtx;
                }
            }
        }

    }
}
