# pipeline.debug

pipeline.debug is a tool for debugging Sitecore pipelines. Once it's on the server, you can add processors to any Sitecore pipeline, and output properties and fields from the pipeline's arguments and Sitecore.Context.

After installation, simply go to [scheme]://[host]/sitecore/admin/pipelinedebug.html and you can start setting up your debugging session.

## adding processors to a pipeline

Go to the Pipelines window to add new processors. Simply choose the pipeline you wish to add a new (or move an existing) debugprocessor to. Then you can simply drag and drop debugprocessors to a given spot in the pipeline.

![Dragging a new processor to the httpRequestBegin pipeline](PipelineDebug/Documentation/add-processor.gif)