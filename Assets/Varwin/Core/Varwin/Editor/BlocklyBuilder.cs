using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public class BlocklyBuilder : MonoBehaviour
    {
        private static BlocklyConfig _config;
        private static Type _wrapperType;
        
        private static Regex _nullableRegex = new Regex(@"System\.Nullable`1\[\[([A-Za-z_.?0-9]+?), .*?\]\]");
        private static Regex _dictionaryRegex = new Regex(@"([A-Za-z_.?0-9]+?)`2\[\[([A-Za-z_.?0-9]+?),.*?\],\[([A-Za-z_.?0-9]+?),.*?\]\]");
        private static Regex _listRegex = new Regex(@"([A-Za-z_.?0-9]+?)`1\[\[([A-Za-z_.?0-9]+?),.*?\]\]");
        
        public class MethodLocale
        {
            public MethodInfo MemberInfo;
            public LocaleAttribute[] LocaleAttributies;
        }

        public static string CreateBlocklyConfig(Wrapper wrapper, Type type, ObjectBuildDescription objectBuild)
        {
            if (!Initialize(wrapper, type, objectBuild))
            {
                return null;
            }

            AddPropertiesToConfig();
            AddFieldsToConfig();
            AddMethodsToConfig();
            AddEventsToConfig();

            var jsonSerializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            string jsonConfig = JsonConvert.SerializeObject(_config, Formatting.None, jsonSerializerSettings);
            return jsonConfig;
        }

        private static bool Initialize(Wrapper wrapper, Type type, ObjectBuildDescription objectBuild)
        {
            if (string.IsNullOrWhiteSpace(type.FullName))
            {
                return false;
            }

            _wrapperType = wrapper.GetType();
            Debug.Log($"<Color=Green><b>Wrapper {_wrapperType.Name} is loaded!</b></Color>");

            VarwinObjectDescriptor varwinObjectDescriptor = objectBuild.ContainedObjectDescriptor;
            Debug.Log($"Core version is {VarwinVersionInfo.VarwinVersion}");

            string builtAt = $"{DateTimeOffset.UtcNow:s}Z";
            if (DateTimeOffset.TryParse(varwinObjectDescriptor.BuiltAt, out DateTimeOffset builtAtDateTimeOffset))
            {
                builtAt = $"{builtAtDateTimeOffset.UtcDateTime:s}Z";
            }
            
            _config = new BlocklyConfig
            {
                Guid = varwinObjectDescriptor.Guid, 
                RootGuid = varwinObjectDescriptor.RootGuid, 
                Embedded = varwinObjectDescriptor.Embedded, 
                MobileReady = SdkSettings.MobileFeature.Enabled && varwinObjectDescriptor.MobileReady,
                Config = new Config 
                {
                    type = $"{varwinObjectDescriptor.Name}_{varwinObjectDescriptor.RootGuid.Replace("-", "")}.{varwinObjectDescriptor.Name}Wrapper", 
                    blocks = new List<Block>(),
                },
                Author = new JsonAuthor
                {
                    Name = varwinObjectDescriptor.AuthorName,
                    Email = varwinObjectDescriptor.AuthorEmail,
                    Url = varwinObjectDescriptor.AuthorUrl,
                },
                BuiltAt = $"{builtAt}",
                License = new JsonLicense
                {
                    Code = varwinObjectDescriptor.LicenseCode,
                    Version =  varwinObjectDescriptor.LicenseVersion,
                },
                SdkVersion = VarwinVersionInfo.VersionNumber
            };
            
            if (File.Exists(objectBuild.TagsPath))
            {
                var tags = File.ReadAllLines(objectBuild.TagsPath);
                _config.Tags = tags.ToList();
            }

            if (_config.Config.i18n == null)
            {
                _config.Config.i18n = new I18n();
            }
            var locales = type.GetCustomAttributes<LocaleAttribute>(true);
            foreach (var locale in locales)
            {
                if (locale.I18n != null)
                {
                    _config.Config.i18n = locale.I18n;
                    break;
                }
                else
                {
                    _config.Config.i18n.SetLocale(locale.Code, locale.Strings[0]);
                }
            }
            return true;
        }

        private static void AddPropertiesToConfig()
        {
            foreach (PropertyInfo property in _wrapperType.GetProperties())
            {
                var attributes = property.GetCustomAttributes(true);
                var locales = property.GetCustomAttributes<LocaleAttribute>(true);

                foreach (object attribute in attributes)
                {
                    Block block;
                    switch (attribute)
                    {
                        case GetterAttribute getter:
                            block = GetBlock(getter.Name, "getter");
                            break;
                        case SetterAttribute setter:
                            block = GetBlock(setter.Name, "setter");
                            break;
                        default:
                            continue;
                    }
                    block.valueType = GetValidTypeName(property.PropertyType);
                    
                    var item = new Item {property = property.Name};
                    
                    foreach (var locale in locales)
                    {
                        if (locale.I18n != null)
                        {
                            item.i18n = locale.I18n;
                            break;
                        }
                        else
                        {
                            if (locale.Strings.Length == 1)
                            {
                                item.SetLocale(locale.Code, locale.Strings[0]);
                            }
                            else
                            {
                                Debug.LogError("Property locale attribute must have 1 string");
                            }
                        }
                    }

                    block.AddItem(item);
                }
            }
        }

        private static void AddFieldsToConfig()
        {
            foreach (FieldInfo field in _wrapperType.GetFields())
            {
                var attributes = field.GetCustomAttributes<ValueAttribute>(true);
                var locales = field.GetCustomAttributes<LocaleAttribute>();

                foreach (ValueAttribute valueAttribute in attributes)
                {
                    Block block = GetBlock(valueAttribute.Name, "values");
                    var item = new Item {name = field.Name};
                    
                    foreach (var locale in locales)
                    {
                        if (locale.I18n != null)
                        {
                            item.i18n = locale.I18n;
                            break;
                        }
                        else
                        {
                            if (locale.Strings.Length == 1)
                            {
                                item.SetLocale(locale.Code, locale.Strings[0]);
                            }
                            else
                            {
                                Debug.LogError("Property locale attribute must have 1 string");
                            }
                        }
                    }

                    block.AddItem(item);
                }
            }
        }

        private static void AddMethodsToConfig()
        {
            foreach (MethodInfo method in _wrapperType.GetMethods())
            {
                var attributes = method.GetCustomAttributes(true);
                var locales = method.GetCustomAttributes<LocaleAttribute>().ToArray();
                foreach (object attribute in attributes)
                {
                    Block block;
                    Item item = new Item {method = method.Name};
                    
                    switch (attribute)
                    {
                        case CheckerAttribute checker:
                            if (method.ReturnType != typeof(bool))
                            {
                                Debug.LogError($"Method {method.Name} must return bool!");
                                continue;
                            }
                            block = GetBlock(checker.Name, "checker");
                            break;
                        case ActionAttribute action:
                            block = GetBlock(action.Name, "action");
                            break;
                        default:
                            continue;
                            break;
                    }
                    
                    foreach (var locale in locales)
                    {
                        if (locale.I18n != null)
                        {
                            item.i18n = locale.I18n;
                            break;
                        }
                        else
                        {
                            if (locale.Strings.Length > 0)
                            {
                                item.SetLocale(locale.Code, locale.Strings[0]);
                            }
                        }
                    }

                    int i = 1;

                    var parameters = method.GetParameters();
                    if (parameters.Length > 0)
                    {
                        var args = new List<Arg>();
                        foreach (ParameterInfo param in parameters)
                        {
                            var arg = new Arg {valueType = GetValidTypeName(param.ParameterType)};

                            int j = 0;
                            foreach (var locale in locales)
                            {
                                if (locale.I18n != null)
                                {
                                    if (j == i)
                                    {
                                        arg.i18n = locale.I18n;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (locale.Strings.Length > i)
                                    {
                                        arg.SetLocale(locale.Code, locale.Strings[i]);
                                    }
                                }
                                j++;
                            }

                            int k = 1;
                            foreach (object atr in attributes)
                            {
                                if (atr is ValuesAttribute valuesAttribute)
                                {
                                    if (i == k)
                                    {
                                        arg.values = valuesAttribute.Name;
                                    }

                                    k++;
                                }
                            }

                            args.Add(arg);

                            i++;
                        }

                        if (block.args == null)
                        {
                            block.args = new List<Arg>();
                            block.args.AddRange(args);
                        }
                        else
                        {
                            if (block.args.Count != args.Count)
                            {
                                Debug.LogError("Count of arguments for methods with same actions is not equals");
                            }
                        }
                    }

                    block.AddItem(item);
                }
            }
        }

        private static void AddEventsToConfig()
        {
            var eventDelegates = new Dictionary<string, MethodLocale>();

            foreach (EventInfo eventInfo in _wrapperType.GetEvents())
            {
                var attributes = eventInfo.GetCustomAttributes<EventAttribute>(true);
                var locales = eventInfo.GetCustomAttributes<LocaleAttribute>();

                foreach (EventAttribute eventAttribute in attributes)
                {
                    Block block = GetBlock(eventAttribute.Name, "event");
                    var item = new Item {method = eventInfo.Name};

                    foreach (var locale in locales)
                    {
                        if (locale.I18n != null)
                        {
                            item.i18n = locale.I18n;
                            break;
                        }
                        else
                        {
                            if (locale.Strings.Length > 0)
                            {
                                item.SetLocale(locale.Code, locale.Strings[0]);
                            }
                        }
                    }

                    block.items.Add(item);
                    Type delegateType = eventInfo.EventHandlerType;
                    MethodInfo method = delegateType.GetMethod("Invoke");

                    if (!eventDelegates.ContainsKey(eventAttribute.Name))
                    {
                        if (method != null)
                        {
                            if (method.DeclaringType != null)
                            {
                                var localeAttributes = method.DeclaringType.GetCustomAttributes<LocaleAttribute>();
                                var methodLocale = new MethodLocale
                                {
                                    MemberInfo = method, 
                                    LocaleAttributies = localeAttributes.ToArray()
                                };

                                eventDelegates.Add(eventAttribute.Name, methodLocale);
                            }
                        }
                    }
                }
            }

            foreach (var eventDelegate in eventDelegates)
            {
                Block block = GetBlock(eventDelegate.Key);
                int i = 0;

                foreach (ParameterInfo param in eventDelegate.Value.MemberInfo.GetParameters())
                {
                    if (block.@params == null)
                    {
                        block.@params = new List<Param>();
                    }

                    var paramBlockly = new Param
                    {
                        name = param.Name, 
                        valueType = GetValidTypeName(param.ParameterType)
                    };

                    int j = 0;
                    foreach (var locale in eventDelegate.Value.LocaleAttributies)
                    {
                        if (locale.I18n != null)
                        {
                            if (i == j)
                            {
                                paramBlockly.i18n = locale.I18n;
                                break;
                            }
                        }
                        else
                        {
                            if (locale.Strings.Length > i)
                            {
                                paramBlockly.SetLocale(locale.Code, locale.Strings[i]);
                            }
                        }

                        j++;
                    }

                    block.@params.Add(paramBlockly);

                    i++;
                }
            }
        }
        
        public static string GetValidTypeName(Type type)
        {
            string typeName = type.FullName;
            
            if (_nullableRegex.IsMatch(typeName))
            {
                typeName = _nullableRegex.Replace(typeName, "$1?");
            }
            
            if (_dictionaryRegex.IsMatch(typeName))
            {
                typeName = _dictionaryRegex.Replace(typeName, "$1<$2,$3>");
            }
            
            if (_listRegex.IsMatch(typeName))
            {
                typeName = _listRegex.Replace(typeName, "$1<$2>");
            }

            string assemblyName = type.Assembly.GetName().Name;
            if (!assemblyName.StartsWith("VarwinCore") && !assemblyName.StartsWith("UnityEngine") && !assemblyName.StartsWith("mscorlib") && !assemblyName.StartsWith("System"))
            {
                throw new Exception($"{typeName}: It is forbidden to use custom types in methods, properties and events.");
            }
            
            return typeName;
        }

        private static bool IsValuesArgAdded(List<Arg> args)
        {
            foreach (Arg arg in args)
            {
                if (arg.values != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static Block GetBlock(string blockName, string blockType = null, string valueType = null)
        {
            foreach (Block block in _config.Config.blocks)
            {
                if (string.Equals(block.name, blockName, StringComparison.Ordinal))
                {
                    return block;
                }
            }

            var newBlock = new Block
            {
                name = blockName, 
                items = new List<Item>(),
                type = blockType,
                valueType = valueType
            };
            _config.Config.blocks.Add(newBlock);

            return newBlock;
        }
    }
}
