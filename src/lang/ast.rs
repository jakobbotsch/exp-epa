#[derive(Clone, Debug, PartialEq, Eq)]
pub struct Def {
    pub name: String,
    pub args: Vec<(String, Ty)>,
    pub tm: Tm,
}

#[derive(Clone, Debug, PartialEq, Eq)]
pub enum Tm {
    Typed { tm: Box<Tm>, ty: Box<Ty> },
    App { fun: String, args: Vec<Tm> },
    Let { body: Vec<Def>, result: Box<Tm> },
    True,
    False,
    Neg(Box<Tm>),
    BoolElim {
        discriminee: Box<Tm>,
        into_var: String,
        into_ty: Box<Ty>,
        true_case: Box<Tm>,
        false_case: Box<Tm>,
    },
    Refl(Box<Tm>),
}


#[derive(Clone, Debug, PartialEq, Eq)]
pub enum Ty {
    Bool,
    Eq(Box<Tm>, Box<Tm>),
}
