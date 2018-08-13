@SET dest=".\release"

robocopy .\PipelineDebug\sitecore %dest%\sitecore *.* /E
robocopy .\PipelineDebug\bin %dest%\bin *PipelineDebug*
robocopy .\PipelineDebug\App_Config %dest%\App_Config *.* /E
