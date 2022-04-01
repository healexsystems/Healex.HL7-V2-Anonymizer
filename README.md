![Healex](images/healex-icon-cropped.png)

# Healex.HL7v2Anonymizer

This console application allows you to anonymize HL7v2 messages. The standard configuration anonymizes all identifiable data in HL7 v2 messages and hashes fields that include an ID.

## Motivation

The project was built to enable anyone to share HL7v2 sample messages without identifiable data. 

## How to use?

**Warning: This application overwrites the original message so make sure you are working on a copy.**

1. Download the latest release to a location of your choice.
2. Unzip it.
3. Make sure to back them up prior to runing the application since the original messages will be overwritten.
4. Run the application with `anonymizer -d PathToDirectory` or `anonymizer --directory PathToDirectory`
   1. Do note, that `-d` is a required parameter.
   2. The following other parameters are supported
      1. `--help` displays a help screen that shows all supported command line arguments
      2. `--version` Displays version information

## Configuration

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

Ideally, the value corresponds to its semantic - so if you want a date to be replaced, give it a random date like `01.01.2020` as opposed to entering a random value.
You may also want to pay attention to any character limits for fields and values specified by the HL7 v2 version you are using. Depending on the HL7 v2 version, replacements could otherwise render the message invalid according to that version. For instance, in version 2.5 the `NK1.2.2` field only allows a maximum of 30 characters. 

Use the "HASH" keyword to generate persistent, pseudonymized IDs. This function will always generate the same anonymized ID for a given ID in the HL7 v2 message. The hash function is one-way, so there is no way of reversing the pseudonymized ID back to its original ID.

```json
    {
        "Segment": "PID",
        "Replacements": [
            // ommited
            {
                "Path": "PID.1.1",
                "Value": "HASH" <---- The value in PID.1.1 will be hashed, not overwritten
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

Run the application again for the changes to take effect.
