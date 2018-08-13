# pipeline.debug

pipeline.debug is a tool for debugging Sitecore pipelines. Once it's on the server, you can add processors to any Sitecore pipeline, and output properties and fields from the pipeline's arguments and Sitecore.Context.

The tool is intended for Sitecore developers with basic knowledge of the Sitecore Pipeline architecture and reflection of Sitecore's code in order to debug complex bugs.

Installation can happen by cloning this repository and make a build. Then copy contents of the /App_Config and /sitecore folders, and copy the PipelineDebug.dll to the bin folder. I will add it to the Sitecore Marketplace as well.

After installation, simply go to [scheme]://[host]/sitecore/admin/pipelinedebug.html and you can start setting up your debugging session.

## Adding processors to a pipeline

Go to the Pipelines window to add new processors. Simply choose the pipeline you wish to add a new (or move an existing) debugprocessor to. Then you can simply drag and drop debugprocessors to a given spot in the pipeline.

In the below example we add a debugprocessor to the httpRequestBegin pipeline.
![Dragging a new processor to the httpRequestBegin pipeline](PipelineDebug/Documentation/add-processor.gif)

It's possible to add several processors to one pipeline, as well as adding processors to multiple pipelines at the same time.

## Setting up which values to output

Each configured debugprocessor has an icon to open the edit overlay. This overlay let's you decide which values you wish to output from the pipelines arguments and Sitecore.Context. At the top the currently selected values are shown. A newly added processor will automatically have any default taxonomies from settings added. You can add additional taxonomies either by writing them in the textboxes (once selected, a new textbox will pop up), or by using the discovery feature at the bottom. 

In the below example I choose that I wish to output the cookies of the request in my httpRequestBegin pipeline.
![Selecting taxonomies via the discovery feature](PipelineDebug/Documentation/add-processor.gif)

The discovery uses reflection to find all properties and fields (public or private) of the given types. The list is however not necesarily exhaustive, since we can only find the members of the declared types, and not possible inherited types used in it's stead. This is for instance clearly displayed when using Sitecore Commerce's ServicePipelineArgs, where the ServiceProviderRequest and ServiceProviderResult is always an inherited class with context-specific members. This problem only exists at discovery time. When the debugprocessor is run, it will run on the runtime type, and all members will be found. This means that any taxonomies added via the textboxes, will work - this will however require you to do some reflection on your own, in order to find the values.

## Managing configured processors

The Processors window gives an overview of all active processors. Here you can easily edit or remove each processor.

![The processors overview](PipelineDebug/Documentation/processors.png)