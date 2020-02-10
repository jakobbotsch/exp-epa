use super::cwf::*;

pub trait Model {
    fn ctx_eq(&self, l: &Ctx, r: &Ctx) -> bool;
    fn morph_eq(&self, l: &Morph, r: &Morph) -> bool;
    fn ty_eq(&self, l: &Ty, r: &Ty) -> bool;
    fn tm_eq(&self, l: &Tm, r: &Tm) -> bool;

    fn empty_ctx(&mut self) -> &Ctx;
    fn comprehension(&mut self, base: &Ctx, ty: &Ty) -> &Ctx;
    fn project_ctx(&mut self, ctx: &Ctx, ty: &Ty) -> &Morph;
    fn project_tm(&mut self, ctx: &Ctx, ty: &Ty) -> &Tm;

    fn id_morph(&mut self, ctx: &Ctx) -> &Morph;
    fn compose(&mut self, g: &Morph, f: &Morph) -> &Morph;
    fn extension(&mut self, morph: &Morph, tm: &Tm, ty: &Ty) -> &Morph;

    fn subst_ty(&mut self, base: &Ty, morph: &Morph) -> &Ty;
    fn subst_tm(&mut self, base: &Tm, morph: &Morph) -> &Tm;

    fn eq(&mut self, l: &Tm, r: &Tm) -> &Ty;
    fn refl(&mut self, tm: &Tm) -> &Tm;

    fn bool(&mut self, ctx: &Ctx) -> &Ty;
    fn true_tm(&mut self, ctx: &Ctx) -> &Tm;
    fn false_tm(&mut self, ctx: &Ctx) -> &Tm;
    fn elim_bool(&mut self, into: &Ty, true_case: &Tm, false_case: &Tm) -> &Tm;
}