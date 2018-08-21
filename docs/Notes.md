# Work notes

## Ideas

Main entity in project is `Resource`, that must be lightweight and simple.  
`Resource` is observable by `ResourcePipeleine`. This observarble created and served by `ResourceProducer`.  
One type of `Resource` can be produced couple of `ResourceProducer`,  so resource should have `ProducerHash` of producer, that define 'who create this resource'.  One type of producer must have different `Hash` for different configs (params) that used in generate resources.
For example `File : Resource<string>` represents file on disk (where `string Value` is filepath), this resource can be produced by `DirectoryWatcher : IResourceProducer<File>` or RP like this `UploadedFiles : IResourceProducer<File>`.  If we want create 2 `DirectoryWatcher`they must have two different `Hash`
