using System;
using System.Collections.Generic;
using System.Linq;

namespace QT
{
    internal class Rules
    {
        public Rules()
        {
            var CtxEq = Rel<CtxS, CtxS>("CtxEq");
            var CtxMorphEq = Rel<CtxMorphS, CtxMorphS>("CtxMorphEq");
            var TyEq = Rel<TyS, TyS>("TyEq");
            var TmEq = Rel<TmS, TmS>("TmEq");
            var Ctx = Rel<CtxS>("Ctx");
            var CtxMorph = Rel<CtxMorphS, CtxS, CtxS>("CtxMorph");
            var Ty = Rel<TyS, CtxS>("Ty");
            var Tm = Rel<TmS, TyS>("Tm");
            var IdMorph = Rel<CtxMorphS>("IdMorph", functional: true);
            var Comp = Rel<CtxMorphS, CtxMorphS, CtxMorphS>("Comp", functional: true);
            var TySubst = Rel<TyS, CtxMorphS, TyS>("TySubst", functional: true);
            var TmSubst = Rel<TmS, CtxMorphS, TmS>("TmSubst", functional: true);
            var CtxEmpty = Rel<CtxS>("CtxEmpty", functional: true);
            var Comprehension = Rel<CtxS, TyS, CtxS>("Comprehension", functional: true);
            var ProjCtx = Rel<CtxS, TyS, CtxMorphS>("ProjCtx", functional: true);
            var ProjTm = Rel<CtxS, TyS, TmS>("ProjTm", functional: true);
            var Extension = Rel<CtxMorphS, TmS, CtxMorphS>("Extension", functional: true);
            var Id = Rel<TmS, TmS, TyS>("Id", functional: true);
            var Refl = Rel<TmS>("Refl", functional: true);
            var Bool = Rel<TyS>("Bool", functional: true);
            var True = Rel<TmS>("True", functional: true);
            var False = Rel<TmS>("False", functional: true);
            var BoolElim = Rel<TmS, TmS, TmS>("BoolElim", functional: true);
            var TmBar = Rel<TmS, CtxMorphS>("TmBar");

            var relations = new List<Relation>
            {
                CtxEq, CtxMorphEq, TyEq, TmEq, Ctx, CtxMorph, Ty, Tm,
                IdMorph, Comp, TySubst, TmSubst, CtxEmpty, Comprehension,
                ProjCtx, ProjTm, Extension, Id, Refl, Bool, True, False, BoolElim, TmBar
            };

            var A = new CtxS();
            var B = new CtxS();
            var C = new CtxS();
            var D = new CtxS();
            var E = new CtxS();
            var F = new CtxS();
            var G = new CtxS();

            var f = new CtxMorphS();
            var g = new CtxMorphS();
            var h = new CtxMorphS();
            var gf = new CtxMorphS();
            var hg = new CtxMorphS();
            var hgf = new CtxMorphS();
            var gid = new CtxMorphS();
            var id = new CtxMorphS();

            var s = new TyS();
            var t = new TyS();
            var u = new TyS();
            var v = new TyS();
            var x = new TyS();
            var y = new TyS();

            var M = new TmS();
            var N = new TmS();
            var O = new TmS();
            var P = new TmS();
            var Q = new TmS();
            var R = new TmS();
            var S = new TmS();
            var T = new TmS();
            var U = new TmS();

            var rules = new List<HornClause>
            {
                // g . id = g
                CtxMorphEq[gid, g] <= CtxMorph[g, G, D] +
                                      CtxMorph[id, G, G] + IdMorph[id] +
                                      Comp[g, id, gid],
            };

            MakeEquality(CtxEq, G => Ctx[G], relations, rules);
            MakeEquality(CtxMorphEq, f => CtxMorph[f, G, D], relations, rules);
            MakeEquality(TyEq, s => Ty[s, G], relations, rules);
            MakeEquality(TmEq, M => Tm[M, s], relations, rules);
            MakeFunctions(relations, rules);
        }

        private static Relation<T1> Rel<T1>(string name, bool functional = false) where T1 : Sort
            => new Relation<T1> { Name = name, IsFunctional = functional };
        private static Relation<T1, T2> Rel<T1, T2>(string name, bool functional = false) where T1 : Sort where T2 : Sort
            => new Relation<T1, T2> { Name = name, IsFunctional = functional };
        private static Relation<T1, T2, T3> Rel<T1, T2, T3>(string name, bool functional = false) where T1 : Sort where T2 : Sort where T3 : Sort
            => new Relation<T1, T2, T3> { Name = name, IsFunctional = functional };

        private static void MakeEquality<T>(
            Relation<T, T> equiv,
            Func<T, AppliedRelationExpr> baseRel,
            IEnumerable<Relation> rels,
            List<HornClause> rules) where T : Sort, new()
        {
            var a = new T();
            var b = new T();
            var c = new T();
            rules.Add(equiv[a, a] <= baseRel(a)); // reflexivity
            rules.Add(equiv[a, b] <= equiv[b, a]); // symmetry
            rules.Add(equiv[a, c] <= equiv[a, b] + equiv[b, c]); // transitivity

            // Generate congruence rules
            foreach (Relation rel in rels)
            {
                if (rel == equiv)
                    continue;

                Type[] sorts = rel.GetType().GetGenericArguments();
                // Args in head of rule, eg. for
                // Tm(M, s) :- Tm(M, t), TyEq(s, t)
                // the head args will be M, s
                // the body args will be M, t
                // and the preconditions will be [TyEq(s, t)]
                List<Sort> headArgs = new List<Sort>();
                List<Sort> bodyArgs = new List<Sort>();
                List<PropExpr> preconditions = new List<PropExpr>();
                foreach (Type t in sorts)
                {
                    if (t == typeof(T))
                    {
                        var v1 = new T();
                        var v2 = new T();
                        headArgs.Add(v1);
                        bodyArgs.Add(v2);
                        preconditions.Add(equiv[v1, v2]);
                    }
                    else
                    {
                        var v = Activator.CreateInstance(t);
                        headArgs.Add((Sort)v!);
                        bodyArgs.Add((Sort)v!);
                    }
                }

                if (preconditions.Count > 0)
                {
                    rules.Add(
                        new HornClause(new AppliedRelationExpr(rel, headArgs),
                        preconditions.Aggregate(
                            (PropExpr)new AppliedRelationExpr(rel, bodyArgs),
                            (acc, res) => acc + res)));
                }
            }
        }

        private static void MakeFunctions(List<Relation> rels, List<HornClause> rules)
        {
            foreach (Relation rel in rels.Where(r => r.IsFunctional))
            {
                // TmEq[M, N] :- rel[a, b, ..., M], rel[a, b, ..., N]
                Type ty = rel.GetType().GetGenericArguments().Last();
                Relation equality = rels.Single(r => r.IsEquality && r.GetType().GetGenericArguments().All(t => t == ty));

                var v1 = (Sort)Activator.CreateInstance(ty)!;
                var v2 = (Sort)Activator.CreateInstance(ty)!;
                List<Sort> args = rel.GetType().GetGenericArguments().SkipLast(1).Select(t => (Sort)Activator.CreateInstance(t)!).ToList();

                AppliedRelationExpr first = new AppliedRelationExpr(rel, args.Concat(new[] { v1 }).ToList());
                AppliedRelationExpr second = new AppliedRelationExpr(rel, args.Concat(new[] { v2 }).ToList());
                rules.Add(new HornClause(new AppliedRelationExpr(equality, v1, v2), first + second));
            }
        }
    }

    internal class Sort { }
    internal class CtxS : Sort { }
    internal class CtxMorphS : Sort { }
    internal class TyS : Sort { }
    internal class TmS : Sort { }

    internal abstract class Relation
    {
        public string? Name { get; set; }
        public bool IsFunctional { get; set; }
        public bool IsEquality { get; set; }
    }

    internal class Relation<T1> : Relation where T1 : Sort
    { public AppliedRelationExpr this[T1 s1] => new AppliedRelationExpr(this, s1); }
    internal class Relation<T1, T2> : Relation where T1 : Sort where T2 : Sort
    { public AppliedRelationExpr this[T1 s1, T2 s2] => new AppliedRelationExpr(this, s1, s2); }
    internal class Relation<T1, T2, T3> : Relation where T1 : Sort where T2 : Sort where T3 : Sort
    { public AppliedRelationExpr this[T1 s1, T2 s2, T3 s3] => new AppliedRelationExpr(this, s1, s2, s3); }

    internal abstract class PropExpr
    {
        public static AndExpr operator &(PropExpr l, PropExpr r)
            => new AndExpr(l, r);

        public static AndExpr operator +(PropExpr l, PropExpr r)
            => new AndExpr(l, r);
    }

    internal class AppliedRelationExpr : PropExpr
    {
        public AppliedRelationExpr(Relation rel, IReadOnlyList<Sort> args)
        {
            Rel = rel;
            Args = args;
        }

        public AppliedRelationExpr(Relation rel, params Sort[] args) : this(rel, (IReadOnlyList<Sort>)args)
        {
        }

        public Relation Rel { get; }
        public IReadOnlyList<Sort> Args { get; }

        public static HornClause operator <=(AppliedRelationExpr head, PropExpr body)
            => new HornClause(head, body);

        public static HornClause operator >=(AppliedRelationExpr head, PropExpr body)
            => throw new InvalidOperationException();
    }

    internal class AndExpr : PropExpr
    {
        public AndExpr(PropExpr left, PropExpr right)
        {
            Left = left;
            Right = right;
        }

        public PropExpr Left { get; }
        public PropExpr Right { get; }
    }

    internal class HornClause
    {
        public HornClause(AppliedRelationExpr head, PropExpr body)
        {
            Head = head;
            Body = body;
        }

        public AppliedRelationExpr Head { get; }
        public PropExpr Body { get; }
    }
}
