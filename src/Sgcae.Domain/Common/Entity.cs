namespace Sgcae.Domain.Common;

// EN: This is the base class for every entity. An entity is identified by its Id, not by its attribute values.
// ES: Esta es la clase base de todas las entidades. Una entidad se identifica por su Id, no por el valor de sus atributos.
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    // EN: Here I compare two entities using only their identity (Id).
    // ES: Aquí comparo dos entidades usando únicamente su identidad (Id).
    public bool Equals(Entity? other) =>
        other is not null && other.GetType() == GetType() && other.Id == Id;

    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => Id.GetHashCode();
}

// EN: This marker class identifies the aggregate roots: the only entry point to modify an aggregate.
// ES: Esta clase marcadora identifica las raíces de agregado: el único punto de entrada para modificar un agregado.
public abstract class AggregateRoot : Entity
{
}
