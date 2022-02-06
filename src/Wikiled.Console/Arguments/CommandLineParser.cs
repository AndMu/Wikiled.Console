using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Wikiled.Console.Arguments
{
    /// <summary>
    ///     Provides utilities for parsing command-line values.
    /// </summary>
    /// <example>
    ///     The following example shows parsing of a command-line such as "Test.exe /verbose /runId=10"
    ///     into a strongly-typed structure.
    ///     <code>
    ///  using System;
    ///  using System.Linq;
    ///  using Microsoft.Test.CommandLineParsing;
    ///  
    ///  public class CommandLineArguments
    ///  {
    ///      public bool? Verbose { get; set; }
    ///      public int? RunId { get; set; }
    ///  }
    /// 
    ///  public class Program
    ///  {
    ///      public static void Main(string[] args)
    ///      {
    ///          CommandLineArguments a = new CommandLineArguments();
    ///          a.ParseArguments(args);  // or CommandLineParser.ParseArguments(a, args);
    ///          
    ///          Console.WriteLine("Verbose: {0}, RunId: {1}", a.Verbose, a.RunId);
    ///      }
    ///  }
    ///  </code>
    /// </example>
    /// <example>
    ///     The following example shows parsing of a command-line such as "Test.exe RUN /verbose /runId=10"
    ///     into a strongly-typed Command, that can then be excuted.
    ///     <code>
    ///  using System;
    ///  using System.Linq;
    ///  using Microsoft.Test.CommandLineParsing;
    ///  
    ///  public class RunCommand : Command
    ///  {
    ///      public bool? Verbose { get; set; }
    ///      public int? RunId { get; set; }
    /// 
    ///      public override void Execute()
    ///      {
    ///          Console.WriteLine("RunCommand: Verbose={0} RunId={1}", Verbose, RunId);  
    ///      }
    ///  }
    /// 
    ///  public class Program
    ///  {
    ///      public static void Main(string[] args)
    ///      {
    ///          if (String.Compare(args[0], "run", StringComparison.InvariantCultureIgnoreCase) == 0)
    ///          {
    ///              Command c = new RunCommand();
    ///              c.ParseArguments(args.Skip(1)); // or CommandLineParser.ParseArguments(c, args.Skip(1))
    ///              c.Execute();
    ///          }
    ///      }
    ///  }
    ///  </code>
    /// </example>
    public static class CommandLineParser
    {
        /// <summary>
        ///     Static constructor.
        /// </summary>
        static CommandLineParser()
        {
            // The parser will want to convert from value line string arguments into various
            // data types on a value. Any type that doesn't have a default TypeConverter that
            // can convert from string to it's type needs to have a custom TypeConverter written
            // for it, and have it added here.
            TypeDescriptor.AddAttributes(typeof(DirectoryInfo), new TypeConverterAttribute(typeof(DirectoryInfoConverter)));
            TypeDescriptor.AddAttributes(typeof(FileInfo), new TypeConverterAttribute(typeof(FileInfoConverter)));
        }

        /// <summary>
        ///     Sets properties on an object from a series of key/value string
        ///     arguments that are in the form "/PropertyName=Value", where the
        ///     value is converted from a string into the property type.
        /// </summary>
        /// <param name="valueToPopulate">The object to set properties on.</param>
        /// <param name="args">The key/value arguments describing the property names and values to set.</param>
        /// <returns>
        ///     Indicates whether the properties were successfully set.  Reasons for failure reasons include
        ///     a property name that does not exist or a value that cannot be converted from a string.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when one of the key/value strings cannot be parsed into a property.</exception>
        public static void ParseArguments(this object valueToPopulate, IEnumerable<string> args)
        {
            CommandLineDictionary commandLineDictionary = CommandLineDictionary.FromArguments(args);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(valueToPopulate);
            // Ensure required properties are specified.
            foreach (PropertyDescriptor property in properties)
            {
                // See whether any of the attributes on the property is a RequiredAttribute.
                if (property.Attributes.Cast<Attribute>().Any(attribute => attribute is RequiredAttribute))
                {
                    // If so, and the command line dictionary doesn't contain a key matching
                    // the property's name, it means that a required property isn't specified.
                    if (!commandLineDictionary.ContainsKey(property.Name))
                    {
                        throw new ArgumentException("A value for the " + property.Name + " property is required.");
                    }
                }
            }

            foreach (KeyValuePair<string, string> keyValuePair in commandLineDictionary)
            {
                // Find a property whose name matches the kvp's key, ignoring case.
                // We can't just use the indexer because that is case-sensitive.                
                PropertyDescriptor property = MatchProperty(keyValuePair.Key, properties, valueToPopulate.GetType());

                // If the value is null/empty and the property is a bool, we
                // treat it as a flag, which means its presence means true.
                if (string.IsNullOrEmpty(keyValuePair.Value) && (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?)))
                {
                    property.SetValue(valueToPopulate, true);
                    continue;
                }

                object valueToSet;

                // We support a limited set of collection types. Setting a List<T>
                // is one of the most flexible types as it supports three different
                // interfaces, but the catch is that we don't support the concrete
                // Collection<T> type. We can expand it to support Collection<T>
                // in the future, but the code will get a bit uglier.
                switch (property.PropertyType.Name)
                {
                    case "IEnumerable`1":
                    case "ICollection`1":
                    case "IList`1":
                    case "List`1":
                        MethodInfo methodInfo = typeof(CommandLineParser).GetMethod("FromCommaSeparatedList", BindingFlags.Static | BindingFlags.NonPublic);
                        Type[] genericArguments = property.PropertyType.GetGenericArguments();
                        MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(genericArguments);
                        valueToSet = genericMethodInfo.Invoke(null, new object[] { keyValuePair.Value });
                        break;
                    default:
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(property.PropertyType);
                        if (typeConverter == null || !typeConverter.CanConvertFrom(typeof(string)))
                        {
                            throw new ArgumentException("Unable to convert from a string to a property of type " + property.PropertyType + ".");
                        }

                        valueToSet = typeConverter.ConvertFrom(keyValuePair.Value);
                        break;
                }

                property.SetValue(valueToPopulate, valueToSet);
            }
        }

        /// <summary>
        ///     Prints names and descriptions for properties on the specified component.
        /// </summary>
        /// <param name="component">The component to print usage for.</param>
        public static void PrintUsage(object component)
        {
            IEnumerable<PropertyDescriptor> properties = TypeDescriptor.GetProperties(component).Cast<PropertyDescriptor>();
            IEnumerable<string> propertyNames = properties.Select(property => property.Name);
            IEnumerable<string> propertyDescriptions = properties.Select(property => property.Description);
            IEnumerable<string> lines = FormatNamesAndDescriptions(propertyNames, propertyDescriptions, System.Console.WindowWidth);

            System.Console.WriteLine("Possible arguments:");
            foreach (string line in lines)
            {
                System.Console.WriteLine(line);
            }
        }

        /// <summary>
        ///     Given collections of names and descriptions, returns a set of lines
        ///     where the description text is wrapped and left aligned. eg:
        ///     First Name   this is a string that wraps around
        ///     and is left aligned.
        ///     Second Name  this is another string.
        /// </summary>
        /// <param name="names">Collection of name strings.</param>
        /// <param name="descriptions">Collection of description strings.</param>
        /// <param name="maxLineLength">Maximum length of formatted lines</param>
        /// <returns>Formatted lines of text.</returns>
        private static IEnumerable<string> FormatNamesAndDescriptions(IEnumerable<string> names, IEnumerable<string> descriptions, int maxLineLength)
        {
            if (names.Count() != descriptions.Count())
            {
                throw new ArgumentException("Collection sizes are not equal", "names");
            }

            int namesMaxLength = names.Max(commandName => commandName.Length);

            List<string> lines = new List<string>();

            for (int i = 0; i < names.Count(); i++)
            {
                string line = names.ElementAt(i);
                line = line.PadRight(namesMaxLength + 2);

                foreach (string wrappedLine in WordWrap(descriptions.ElementAt(i), maxLineLength - namesMaxLength - 3))
                {
                    line += wrappedLine;
                    lines.Add(line);
                    line = new string(' ', namesMaxLength + 2);
                }
            }

            return lines;
        }


        /// <summary>
        ///     Match the property to the specified keyName
        ///     If match cannot be found, throw an argument exception
        /// </summary>
        private static PropertyDescriptor MatchProperty(string keyName, PropertyDescriptorCollection properties, Type targetType)
        {
            foreach (PropertyDescriptor prop in properties)
            {
                if (prop.Name.Equals(keyName, StringComparison.OrdinalIgnoreCase))
                {
                    return prop;
                }
            }

            throw new ArgumentException("A matching public property of name " + keyName + " on type " + targetType + " could not be found.");
        }

        /// <summary>
        ///     Word wrap text for a specified maximum line length.
        /// </summary>
        /// <param name="text">Text to word wrap.</param>
        /// <param name="maxLineLength">Maximum length of a line.</param>
        /// <returns>Collection of lines for the word wrapped text.</returns>
        private static IEnumerable<string> WordWrap(string text, int maxLineLength)
        {
            List<string> lines = new List<string>();
            string currentLine = string.Empty;

            foreach (string word in text.Split(' '))
            {
                // Whenever adding the word would push us over the maximum
                // width, add the current line to the lines collection and
                // begin a new line. The new line starts with space padding
                // it to be left aligned with the previous line of text from
                // this column.
                if (currentLine.Length + word.Length > maxLineLength)
                {
                    lines.Add(currentLine);
                    currentLine = string.Empty;
                }

                currentLine += word;

                // Add spaces between words except for when we are at exactly the
                // maximum width.
                if (currentLine.Length != maxLineLength)
                {
                    currentLine += " ";
                }
            }

            // Add the remainder of the current line except for when it is
            // empty, which is true in the case when we had just started a
            // new line.
            if (currentLine.Trim() != string.Empty)
            {
                lines.Add(currentLine);
            }

            return lines;
        }
    }
}
