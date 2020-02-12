type Id = ();

#[derive(PartialEq, Eq, Clone, Hash)]
pub enum CtxPayload {
  Empty,
  Comprehension(Box<Ctx>, Box<Ty>),
}

#[derive(PartialEq, Eq, Clone, Hash)]
pub enum MorphPayload {
  Identity(Box<Ctx>),
  Initial(Box<Ctx>), // Initial morphism from <> -> G
  Weakening(Box<Ctx>), // G -> G.A
  Composition(Box<Morph>, Box<Morph>), // g . f
  // f : G -> D
  // a \in Tm(G)
  // <f, a> : G -> D.fa
  Extension(Box<Morph>, Box<Tm>), // <f, a> for f : D -> G, a \i
}

#[derive(PartialEq, Eq, Clone, Hash)]
pub enum TyPayload {
  Subst(Box<Morph>, Box<Ty>),
  Bool(Box<Ctx>),
  EqTy(Box<Tm>, Box<Tm>),
}

#[derive(PartialEq, Eq, Clone, Hash)]
pub enum TmPayload {
  Subst(Box<Morph>, Box<Tm>),
  Projection(Box<Ctx>),
  True(Box<Ctx>),
  False(Box<Ctx>),
  ElimBool { into: Box<Ty>, true_case: Box<Tm>, false_case: Box<Tm> },
}

#[derive(PartialEq, Eq, Clone, Hash)]
pub struct WithId<T>(pub Id, pub T);

pub type Ctx = WithId<CtxPayload>;
pub type Morph = WithId<MorphPayload>;
pub type Ty = WithId<TyPayload>;
pub type Tm = WithId<TmPayload>;