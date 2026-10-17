namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// What reads, writes and compares one kind of object of the map.
    /// </summary>
    public sealed record MapObjectKit<TModel>(IMapObjectDeserializer<TModel> Deserializer, IMapObjectSerializer<TModel> Serializer, IMapObjectDiffer<TModel> Differ);
}
