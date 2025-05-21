using Ansroled.Common.Extensions;

namespace Ansroled.Common;

public enum Editor
{
    [StringValue("tasks")]
    Tasks,
    
    [StringValue("files")]
    Files,
    
    [StringValue("handlers")]
    Handlers,
    
    [StringValue("defaults")]
    Defaults,
    
    [StringValue("meta")]
    Meta,
    
    [StringValue("vars")]
    Vars,
    
    [StringValue("templates")]
    Templates
};