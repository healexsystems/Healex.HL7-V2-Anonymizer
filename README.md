![Healex](images/healex-icon-cropped.png)

# Healex.HL7v2Anonymizer

This console application allows you to anonymize HL7v2 messages. 

## Motivation

The project was built to enable data stewards and scientists to share HL7v2 sample messages without identifiable data. 

## How to use?

**Warning: This application overwrites the original message so make sure you are working on a copy.**

1. Download the latest release to a location of your choice.
2. Unzip it.
3. Run the application and enter the path to your v2 messages. Make sure to back them up prior to runing the application since the original messages will be overwritten.

This application will use the `appsettings.json` to read the values that are to be replaced for each segments and their corresponding subsegments. 

A segment is recognized by its `"Segment"` property. Each segment contains an array of replacements. A segment's subsegment can be identified by its `"Path"` property inside the replacements array. Subsegments will also have a value property that contains the value by which a value inside a HL7v2 message is to be replaced.

Say for instance, you want to replace the value that is currently assigned for the given name of an `NK1` segment. Navigate to `appsettings.json`, find the `NK1` segment and replace the value for path `"Path": "NK1.2.2"` like this:

```json
    {
        "Segment": "NK1",
        "Replacements": [
            // ommited
            {
                "Path": "NK1.2.2",
                "Value": "Given name" <---- replace this value
            },
            // omitted
        ]
    }
```

Adding additional segments works in a similar manner. Simply add a new segment to the `appsettings.json` after `"Segment": "IN2"`. Make sure to add a comma to the closing brace of this segment so the JSON file remains valid, then use this template to add a new segment.

```json
    {
        "Segment": "SEGMENT_ID",
        "Replacements": [
            {
                "Path": "Path_to_sub_segment1",
                "Value": "Replacement_value1"
            },
            {
                "Path": "Path_to_sub_segment2",
                "Value": "Replacement_value2"
            }
        ]
    }
```

Save and restart application for the changes to take effect.
