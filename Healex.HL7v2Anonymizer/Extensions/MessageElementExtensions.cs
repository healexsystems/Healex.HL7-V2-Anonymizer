using Healex.HL7v2Anonymizer.Services;
using HL7.Dotnetcore;

namespace Healex.HL7v2Anonymizer.Extensions;

public static class MessageElementExtensions
{
    public static bool TrySetValue(this Segment segment, string path, string replacementValue)
    {
        var parts = path.Split(".");
        switch (parts.Length)
            {
                case 2:
                {
                    // field replacement
                    var fieldIndex = int.Parse(parts[1]);
                    var field = segment.Fields(fieldIndex);
                    field.SetValue(replacementValue);
                    return true;
                }
                case 3:
                {
                    // component replacement
                    var fieldIndex = int.Parse(parts[1]);
                    var compIndex = int.Parse(parts[2]);
                    var field = segment.Fields(fieldIndex);
                    // if the field does not exist, we do not need to look any further
                    if (field == null) return false;
                    var component = field.Components(compIndex);
                    component.SetValue(replacementValue);
                    
                    // check for the component in repetitions of the field
                    if (field.HasRepetitions)
                    {
                        foreach (var rep in field.Repetitions())
                        {
                            var repComponent = rep.Components(compIndex);
                            repComponent.SetValue(replacementValue);
                        }
                    }
                    return true;
                }
                case 4:
                {
                    //subcomponent replacement
                    var fieldIndex = int.Parse(parts[1]);
                    var compIndex = int.Parse(parts[2]);
                    var subCompIndex = int.Parse(parts[3]);
                    var field = segment.Fields(fieldIndex);
                    // if the field is null, we do not have to look any further
                    if (field == null) return false;
                    var component = field.Components(compIndex);
                    
                    // if the component is null, it still might be present in a repetition. 
                    // Thus, no return here.
                    if (component != null) 
                    {
                        var subComponent = component.SubComponents(subCompIndex);
                        subComponent.SetValue(replacementValue);
                    }
                    
                    // do the same for repetitions
                    if (field.HasRepetitions)
                    {
                        foreach (var rep in field.Repetitions())
                        {
                            var repComponent = rep.Components(compIndex);
                            if (repComponent != null)
                            {
                                var repSubComponent = repComponent.SubComponents(subCompIndex);
                                repSubComponent.SetValue(replacementValue);
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

    public static void SetValue(this Field field, string value)
    {
        if (field == null) return;
        field.Value = GetReplacement(field.Value, value);

        if (!field.HasRepetitions) return;
        foreach (var rep in field.Repetitions())
        {
            rep.Value = value;
        }
    }

    public static void SetValue(this Component component, string value)
    {
        if (component == null) return;
        component.Value = GetReplacement(component.Value, value);
    }

    public static void SetValue(this SubComponent subComponent, string value)
    {
        if (subComponent == null) return;
        subComponent.Value = GetReplacement(subComponent.Value, value);
    }

    public static string GetReplacement(string originalValue, string replacementValue)
    {
        if (string.IsNullOrEmpty(originalValue)) return "";
        if (replacementValue == "HASH")
        {
            return HashGenerator.HashString(originalValue);
        }

        return replacementValue;
    }
}