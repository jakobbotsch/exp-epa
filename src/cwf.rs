type Id = u32;

#[derive(Debug, PartialEq, Eq)]
pub enum Ctx<'a> {
    Empty(Id),
    Comprehension(Id, &'a Ctx<'a>, &'a Ty<'a>),
}

#[derive(Debug, PartialEq, Eq)]
pub enum Morph<'a> {
    Identity(Id, &'a Ctx<'a>),
    Terminal(Id, &'a Ctx<'a>),
    Projection(Id, &'a Ctx<'a>),
    Composition(Id, &'a Morph<'a>, &'a Morph<'a>),
    Extension(Id, &'a Morph<'a>, &'a Tm<'a>),
}

#[derive(Debug, PartialEq, Eq)]
pub enum Ty<'a> {
    Subst(Id, &'a Ty<'a>, &'a Morph<'a>),
    Bool(Id, &'a Ctx<'a>),
    Eq(Id, &'a Tm<'a>, &'a Tm<'a>),
}

#[derive(Debug, PartialEq, Eq)]
pub enum Tm<'a> {
    Subst(Id, &'a Tm<'a>, &'a Morph<'a>),
    Projection(Id, &'a Ctx<'a>),
    True(Id, &'a Ctx<'a>),
    False(Id, &'a Ctx<'a>),
    ElimBool { id: Id, into: &'a Ty<'a>, true_case: &'a Tm<'a>, false_case: &'a Tm<'a> },
}

pub trait CwfEntity {
    fn get_id(&self) -> Id;
}

impl CwfEntity for Ctx<'_> {
    fn get_id(&self) -> Id {
        match self {
            &Ctx::Empty(id) => id,
            &Ctx::Comprehension(id, _, _) => id,
        }
    }
}

impl CwfEntity for Morph<'_> {
    fn get_id(&self) -> Id {
        match self {
            &Morph::Identity(id, _) => id,
            &Morph::Terminal(id, _) => id,
            &Morph::Projection(id, _) => id,
            &Morph::Composition(id, _, _) => id,
            &Morph::Extension(id, _, _) => id,
        }
    }
}

impl CwfEntity for Ty<'_> {
    fn get_id(&self) -> Id {
        match self {
            &Ty::Subst(id, _, _) => id,
            &Ty::Bool(id, _) => id,
            &Ty::Eq(id, _, _) => id,
        }
    }
}

impl CwfEntity for Tm<'_> {
    fn get_id(&self) -> Id {
        match self {
            &Tm::Subst(id, _, _) => id,
            &Tm::Projection(id, _) => id,
            &Tm::True(id, _) => id,
            &Tm::False(id, _) => id,
            &Tm::ElimBool { id, .. } => id,
        }
    }
}