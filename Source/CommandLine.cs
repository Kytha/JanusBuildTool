// Copyright (c) Kyle Thatcher. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JanusBuildTool {
    public class Option 
    {
        public string Name;
        public string Value;
    }
    class CommandLine 
    {
        private static HashSet<string> _args;
        private static List<Option> _options;

        public static HashSet<string> Args {
            get 
            { 
                if (_args == null)
                {
                    string[] args = Environment.GetCommandLineArgs().OfType<string>().Skip(1).ToArray();
                    for(int i = 0; i < args.Length; i ++) 
                    {
                        args[i] = args[i].Trim(' ').Trim('-');
                    }
                    _args = new HashSet<string>(args);
                    _parse();
                    return _args;
                }
                return _args;
            }
        }

        public static Dictionary<CommandLineAttribute, MemberInfo> GetCommandLineMembers(Type type)
        {
            if (type == null)
                throw new ArgumentNullException();
            var result = new Dictionary<CommandLineAttribute, MemberInfo>();

            var members = type.GetMembers(BindingFlags.Static | BindingFlags.Public);
            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                var attribute = member.GetCustomAttribute<CommandLineAttribute>();
                if (attribute != null)
                {
                    result.Add(attribute, member);
                }
            }
            return result;
        }

        public static List<Option> GetOptions()
        {
            if(_options != null)
                return _options;
            else {
                _parse();
                return _options;
            }
            
        }

        public static string GetHelp(Type type)
        {
            Dictionary<CommandLineAttribute, MemberInfo> members = GetCommandLineMembers(type);

            StringWriter result = new StringWriter();
            result.WriteLine("Usage: JanusBuildTool.exe [options]");
            result.WriteLine("Options:");

            foreach(var pair in members)
            {   
                result.Write("  -" + pair.Key.Name);
                if(!string.IsNullOrEmpty(pair.Key.Hint))
                    result.Write($"={pair.Key.Hint}");
                result.WriteLine();
                if (!string.IsNullOrEmpty(pair.Key.Description))
                    result.WriteLine($"\t{pair.Key.Description.Replace(Environment.NewLine, Environment.NewLine + "\t")}");
                result.WriteLine();
            }
            return result.ToString();
        }
        public static void Configure(Type type) 
        {
            Dictionary<CommandLineAttribute, MemberInfo> members = GetCommandLineMembers(type);

            var options = GetOptions();

            foreach(var pair in members)
            {
                var attribute = pair.Key;
                var member = pair.Value; 
                var option = options.FirstOrDefault(o => string.Equals(o.Name, attribute.Name, StringComparison.OrdinalIgnoreCase));
                
                if(option == null)
                    continue;
                
                object value;
                Type t;
                
                if (member.MemberType == MemberTypes.Field) {
                    t = ((FieldInfo)member).FieldType;
                }
                else if (member.MemberType == MemberTypes.Property) {
                    t = ((PropertyInfo)member).PropertyType;
                }
                else {
                    throw new Exception("Unknown member type.");
                }
                try {
                    if(t == typeof(bool) && string.IsNullOrEmpty(option.Value))
                    {
                        value = true;
                    } 
                    else if (t.IsArray)
                    {
                        var elementType = t.GetElementType();
                        var values = option.Value.Split(',');
                        value = Array.CreateInstance(elementType,values.Length);
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(elementType);
                        for (int i = 0; i < values.Length; i++)
                        {
                            ((Array)value).SetValue(typeConverter.ConvertFromString(values[i]), i);
                        }
                    }
                    else
                    {
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(t);
                        value = typeConverter.ConvertFromString(option.Value);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Failed to convert configuration property {0} value \"{1}\" to type {2}", member.Name, option.Value, type), ex);
                }

                try
                {
                    if (member.MemberType == MemberTypes.Field) {
                        ((FieldInfo)member).SetValue(null, value);
                    }
                    else if (member.MemberType == MemberTypes.Property) {
                        ((PropertyInfo)member).SetValue(null, value);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Failed to set configuration property {0} with argument {1} to value \"{2}\"", member.Name, option.Name, option.Value), ex);
                }
            }
        }

        private static void _parse() 
        {
            var options = new List<Option>();
            foreach(string arg in Args)
            {
               string name;
               string value;
               if(arg.Contains("="))
               {
                string[] split = arg.Split('=');
                if(split.Length != 2) 
                    throw new Exception(string.Format($"Invalid command line argument {arg}"));
                name = split[0];
                value = split[1];
               }  else {
                   name = arg;
                   value = string.Empty;
               }
               options.Add(new Option{Name = name,Value = value});
            }
            _options = options;
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class CommandLineAttribute : Attribute
    {
        public string Name;
        public string Value;
        public string Hint;
        public string Description;

        public CommandLineAttribute(string name, string description)
        {
            if(name.IndexOf('-') != -1)
                throw new Exception("Command line argument cnannot contain '-'.");
            Name = name;
            Description = description;
        }
        public CommandLineAttribute(string name, string hint, string description)
        {
            if (name.IndexOf('-') != -1)
                throw new Exception("Command line argument cannot contain '-'.");
            Name = name;
            Hint = hint;
            Description = description;
        }
    }
}