# JsonSerialization

This .NET package converts between JSON and plain .NET business objects with minimal effort and overhead.  Unless otherwise specified, all of a class's data fields (of all visibility) are serialized to JSON in appropriate formats, and restored to either an object initially constructed with the default constructor (if present) or an uninitialized instance of the appropriate type.

## Basic usage

* Add a reference to JsonSerialization
* Construct a JsonTranslator
* Call MakeJson(your object) to produce a JsonObject with appropriate JSON structure
* Serialize that JsonObject using either ToString (compact) or ToMultilineString (human-readable)
* Deserialize that string into a new JsonObject using JsonObject.Parse
* Convert the new JsonObject into an instance of your type with JsonTranslator.MakeObject<your object type>

## Notes

To produce JsonObjects asynchronously from a Stream, construct a JsonStreamParser around the desired Stream and call ReadObject as desired to asynchronously produce JsonObjects from the Stream.

To exclude a data field (a computation cache, for instance) from serialization, add a JsonIgnore attribute to that data field.

JsonObjects are hierarchical representations of JSON data that can be manipulated outside of JsonTranslator before writing to file and/or before sending to JsonTranslator.  This could be used to, for instance, attach information about the data type.

When an object type has a default (parameterless) constructor, fields of an instance to be serialized will not be included in the serialization if they equal the field values of an instance of the object constructed with the default constructor.

Custom serialization logic can be supplied to a JsonTranslator upon construction by providing an ITranslatorExtensions, perhaps based on the DefaultTranslatorExtensions provided in this package.

## What it doesn't do

This package does not track or check schema versions.  To robustly use this package to store data that can be reliably decoded after changes to the data fields of the .NET business object, or to communicate data between clients that may have different software versions with different data fields of the .NET business object, the user must implement some schema control mechanism, perhaps similar to serialVersionUID in Java.

