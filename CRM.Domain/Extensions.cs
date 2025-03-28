using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Dynamic;
using System.Web;
using System.Collections.Specialized;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace CRM.Domain {
	public static partial class Extensions {
		public static double MetersToMiles(this decimal meters) {
			double miles = (double)meters / 1609.344;
			return Math.Round(miles, 2);
		}

		public static double MetersToMiles(this double meters) {
			return Convert.ToDecimal(meters).MetersToMiles();
		}

		public static string Format(this decimal? value) {
			if(value == null) { return string.Empty; }
			return string.Format("{0:0.00} Miles", value);
		}

		public static string ToString<T>(this T value, string defaultValue) where T : class {
			if(value != null) {
				return value.ToString();
			}
			return defaultValue;
		}

		public static void SetIfNull<T>(this IDictionary<string, object> item, string key, T defaultValue) {
			try {
				if(!item.ContainsKey(key)) {
					item.Add(key, defaultValue);
				}
			} catch {

			}
		}

		public static void SetValue<T>(this IDictionary<string, object> item, string key, T value) {
			try {
				if(item.ContainsKey(key)) {
					item[key] = value;
				} else {
					item.Add(key, value);
				}
			} catch {

			}
		}

		public static ICollection<T> AddRange<T>(this ICollection<T> target, IEnumerable<T> source) {
			if(target == null)
				throw new ArgumentNullException("target");
			if(source == null)
				throw new ArgumentNullException("source");
			foreach(var element in source) {
				target.Add(element);
			}
			return target;
		}

		public static void AddUnique<T>(this List<T> list, T item) {
			if(!list.Contains(item)) {
				list.Add(item);
			}
		}

		public static string GetResponseString(this HttpResponseMessage resp) {
			Task<byte[]> task = resp.Content.ReadAsByteArrayAsync();
			task.Wait();
			return Encoding.UTF8.GetString(task.Result);
		}

		public static T GetJsonResponse<T>(this HttpResponseMessage resp) {
			Task<byte[]> task = resp.Content.ReadAsByteArrayAsync();
			task.Wait();
			return Encoding.UTF8.GetString(task.Result).FromJson<T>();
		}

		public static string ToJson<T>(this T instance) {
			return JsonConvert.SerializeObject(instance);
		}

		public static T FromJson<T>(this string json) {
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static IDictionary<string, object> ToDictionary<T>(this T instance) {
			IDictionary<string, object> dict = new ExpandoObject();
			Type t = typeof(T);
			PropertyInfo[] properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
			object value;
			string strValue;
			Cleanup attr;
			foreach(PropertyInfo p in properties) {
				value = p.GetValue(instance);
				if(value != null && null != (attr = p.GetCustomAttribute<Cleanup>())) {
					strValue = Convert.ToString(value);
					switch(attr.CleanupType) {
						case Cleanup.CleanupTypes.LowerCase:
							strValue = Convert.ToString(value).Trim().ToLower();
							break;
						case Cleanup.CleanupTypes.UpperCase:
							strValue = Convert.ToString(value).Trim().ToUpper();
							break;
						case Cleanup.CleanupTypes.TitleCase:
							strValue = Convert.ToString(value).Trim().ToTitleCase();
							break;
						case Cleanup.CleanupTypes.PhoneNumber:
							strValue = Convert.ToString(value).Trim().CleanPhoneNumber();
							break;
					}
					dict.Add(p.Name, strValue);
				} else {
					dict.Add(p.Name, value);
				}
			}
			return dict;
		}

		public static T GetValue<T>(this IDictionary<string, object> item, string key, T defaultValue) {
			try {
				if(item.ContainsKey(key)) {
					if(item[key] != null) {
						return (T)Convert.ChangeType(item[key], typeof(T));
					}
				}
			} catch(InvalidCastException) {
				try {
					Type t = typeof(T);
					if(t.IsGenericType) {
						return (T)Convert.ChangeType(item[key], t.GenericTypeArguments[0]);
					}
				} catch {
					//Ignore Error
				}
			} catch {
				//Ignore Error
			}
			return defaultValue;
		}

		public static T GetValue<T>(this ExpandoObject obj, string key, T defaultValue) {
			try {
				IDictionary<string, object> item = (IDictionary<string, object>)obj;
				return item.GetValue(key, defaultValue);
			} catch {

			}
			return defaultValue;
		}

		public static T IsNull<T>(this Nullable<T> value, T defaultValue) where T : struct {
			if(value == null) { return defaultValue; }
			return value.Value;
		}

		public static bool HasValue(this string value) {
			return !string.IsNullOrEmpty(value);
		}

		public static bool IsEmpty(this string value) {
			return string.IsNullOrEmpty(value);
		}

		public static int ToInt(this string value) {
			if(value.IsEmpty()) { return -1; }
			return Convert.ToInt32(value);
		}

		public static DateTime? ToDateTime(this string value) {
			if(value.IsEmpty()) { return null; }
			return DateTime.Parse(value);
		}

		public static long ToLong(this string value) {
			if(value.IsEmpty()) { return -1; }
			return Convert.ToInt64(value);
		}

		public static int? ToNullableInt(this string value) {
			try {
				if(value.IsEmpty()) { return null; }
				return Convert.ToInt32(value);
			} catch {
				return null;
			}
		}

		public static bool? ToNullableBool(this string value) {
			try {
				if(value.IsEmpty()) { return null; }
				return Convert.ToBoolean(value);
			} catch {
				return null;
			}
		}

		public static double ToDouble(this string value) {
			if(value.IsEmpty()) { return 0; }
			return Convert.ToDouble(value);
		}

		public static double ToDouble(this decimal? value) {
			if(!value.HasValue) { return 0; }
			return Convert.ToDouble(value);
		}

		public static decimal ToDecimal(this string value) {
			if(value.IsEmpty()) { return 0; }
			return Convert.ToDecimal(value);
		}

		public static decimal ToDecimal(this double value) {
			return Convert.ToDecimal(value);
		}

		public static bool ToBool(this string value) {
			if(value.IsEmpty()) { return false; }
			return value.Matches("^(Yes|True|1)$");
		}

		public static bool ToBool(this bool? value) {
			return value.HasValue && value.Value;
		}

		public static int ToInt(this int? value) {
			if(value.HasValue) {
				return value.Value;
			}
			return -1;
		}

		public static bool IsLocal(this Uri uri) {
			return uri.IsLoopback || uri.DnsSafeHost.Matches("local$");
		}

		public static int? FeetToInches(this string value) {
			if(value.IsEmpty()) { return null; }
			string[] values = value.Split('.');
			int val;
			if(values.Length.In(1, 2)) {
				val = values[0].ToInt() * 12;
				if(values.Length == 2) {
					val += values[1].ToInt();
				}
				return val;
			}
			return null;
		}

		public static string UpperFirstChar(this string value) {
			if(value.IsEmpty()) { return value; }
			return char.ToUpper(value[0]) +
				((value.Length > 1) ? value.Substring(1).ToLower() : string.Empty);
		}

		public static bool In(this int? value, params int[] array) {
			if(!value.HasValue) { return false; }
			return value.Value.In(array);
		}

		public static bool In<T>(this T value, params T[] array) {
			return array.Contains(value);
		}

		public static bool In(this int value, params int[] array) {
			return array.Contains(value);
		}

		public static bool In(this long value, params long[] array) {
			return array.Contains(value);
		}

		public static int Or(this int value, int defaultValue) {
			return value > 0 ? value : defaultValue;
		}

		public static string TryToLower(this string value) {
			if(value.IsEmpty()) { return value; }
			return value.Trim().ToLower();
		}

		public static string CleanPhoneNumber(this string value) {
			if(value.IsEmpty()) { return value; }
			value = Regex.Replace(value, "[^0-9]", ""); //Remove anything that's not a number
			return Regex.Replace(value, "^1", ""); //Remove prefixed 1's
		}

		public static Match Match(this string value, string pattern) {
			return Regex.Match(value, pattern, RegexOptions.IgnoreCase);
		}

		public static bool Matches(this string value, string pattern) {
			if(value.IsEmpty()) { return false; }
			return Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase);
		}

		public static bool RexExEquals(this string value, string compairValue) {
			if(value.IsEmpty()) { return false; }
			return Regex.IsMatch(value, Regex.Escape(compairValue), RegexOptions.IgnoreCase);
		}

		public static string RexExReplace(this string value, string pattern, string replace) {
			if(value.IsEmpty()) { return value; }
			return Regex.Replace(value, pattern, replace, RegexOptions.IgnoreCase);
		}

		public static bool Matches(this string value, string pattern, out string[] groups) {
			groups = null;
			if(value.IsEmpty()) { return false; }
			MatchCollection matches = Regex.Matches(value, pattern, RegexOptions.IgnoreCase);
			List<string> lst = new List<string>();
			foreach(Match match in matches) {
				if(match.Success) {
					foreach(Group grp in match.Groups) {
						lst.Add(grp.Value);
					}
				}
			}
			if(lst.Count > 0) {
				groups = lst.ToArray();
				return true;
			}
			return false;
		}

		public static string ToYesNo(this string value) {
			string[] groups;
			if(value.Matches(@"^(True|False)$", out groups)) {
				return groups[1].Matches("true") ? "Yes" : "No";
			} else {
				return value ?? "";
			}
		}

		/// <summary>
		/// Format String for CSV
		/// </summary>
		public static String CleanCsv(this String value) {
			if(value.IsEmpty()) { return String.Empty; }
			return System.Web.HttpUtility.HtmlEncode(value);
			//String cleaned = newValue.Replace(@"""", @"""""");
			//return String.Format(@"""{0}""", cleaned);
		}


		///<summary>
		/// Convert an object into a dictionary
		///</summary>
		///<param name="object">The object</param>
		///<param name="nullHandler">Handler for null value</param>
		///<returns></returns>
		public static Dictionary<string, object> ClassToDictionary(this Object @object/*, Func<String, Object> nullHandler = null*/ ) {
			if(@object == null) {
				return new Dictionary<string, object>();
			}
			String className = TypeDescriptor.GetClassName(@object).Substring(TypeDescriptor.GetClassName(@object).LastIndexOf('.') + 1);

			var properties = TypeDescriptor.GetProperties(@object);

			var hash = new Dictionary<string, object>(properties.Count);

			foreach(PropertyDescriptor descriptor in properties) {
				var key = descriptor.Name;
				var value = descriptor.GetValue(@object);

				if(value != null) {
					hash.Add(String.Format("{0}", key), value);
				}
				/*else if( nullHandler != null ) {
                    hash.Add( key, nullHandler( key ) );
                }*/
			}

			return hash;
		}

		/// <summary>
		/// Checks if a key exists in the dictionary, and returns the value if it does, otherwise it returns the default return value specified.
		/// </summary>
		/// <typeparam name="TKey">Key type</typeparam>
		/// <typeparam name="TValue">Value type</typeparam>
		/// <param name="dict">Dictionary being extended</param>
		/// <param name="key">Key to check for</param>
		/// <param name="defaultReturnValue">Value to return if specified <paramref name="key"/> is not found</param>
		/// <returns>Returns <typeparamref name="TValue"/> value at <paramref name="key"/> or <paramref name="defaultReturnValue"/> if key does not exist.</returns>
		public static TValue GetIfKeyExists<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultReturnValue) {
			return dict.Keys.Contains(key) ? dict[key] : defaultReturnValue;
		}

		public static string PostedData(this HttpRequestBase req) {
			string data = string.Empty;
			try {
				HttpContext context = HttpContext.Current;
				using(Stream stream = req.InputStream) {
					stream.Position = 0;
					using(StreamReader reader = new StreamReader(stream, req.ContentEncoding)) {
						data = reader.ReadToEnd();
					}
					if(data.IsEmpty()) {
						data = context.Session["PostedData"] as string;
					} else {
						context.Session["PostedData"] = data;
					}
				}
			} catch {
				//Ignore Errors
			}
			return data ?? string.Empty;
		}

		public static T DeserializeJson<T>(this string str) {
			return JsonConvert.DeserializeObject<T>(str);
		}

		public static List<PropertyInfo> GetProperties<T>(this T obj) {
			Type t = obj.GetType();
			return (from prop in t.GetProperties(BindingFlags.Instance | BindingFlags.Public) select prop).ToList();

		}


		public static string SerializeToHtml<T>(this T obj, string[] errorProperties, string[] ignoreProperties) {
			Type t = obj.GetType();
			object value;

			var orderedProperties = from property in t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
									orderby property.Name
									let attr = property.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute
									orderby attr == null ? int.MaxValue : attr.Order
									select new { Name = attr == null ? property.Name : (attr.Name ?? property.Name), Property = property };


			StringBuilder buffer = new StringBuilder();
			foreach(var item in orderedProperties) {
				PropertyInfo prop = item.Property;
				if(ignoreProperties != null && ignoreProperties.Contains(prop.Name)) {
					continue;
				}

				switch(Type.GetTypeCode(prop.PropertyType)) {
					case TypeCode.Object:
						if(prop.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>)) {
							continue;
						}
						value = prop.GetValue(obj);
						break;
					default:
						value = prop.GetValue(obj);
						break;
				}
				if(errorProperties.Contains(prop.Name)) {
					buffer.AppendFormat("<span style='color:#f00;'><b>{0}:</b> {1}</span>", item.Name, value ?? "").AppendLine();
				} else {
					buffer.AppendFormat("<b>{0}:</b> {1}", item.Name, value ?? "").AppendLine();
				}
			}
			return buffer.ToString();
		}

		public static String SerializeCSV<T>(this T obj) {
			return obj.SerializeCSV(true, new StringBuilder());
		}

		private static String SerializeCSV<T>(this T obj, bool includeHeaders, StringBuilder appendTo) {
			Type t;
			PropertyInfo prop;

			bool appendComma = false;
			if(obj is IEnumerable) {
				IEnumerable arry = obj as IEnumerable;
				int count = 0;
				foreach(object item in arry) {
					item.SerializeCSV(includeHeaders && count == 0, appendTo);
					count++;
				}
			} else {
				object value = null;
				t = obj.GetType();
				if(includeHeaders) {
					//Add Headers
					foreach(var p in t.GetProperties()) {
						prop = t.GetProperty(p.Name);
						object[] attrs = prop.GetCustomAttributes(typeof(XmlIgnoreAttribute), false);
						if(attrs.Length >= 1) { continue; }
						value = prop.GetValue(obj);
						if(Type.GetTypeCode(p.PropertyType) == TypeCode.Object) {
							/*
                            if (value != null) {
                                if (value is List<Questions>) {
                                    foreach (var q in value as List<Questions>) {
                                        if (appendComma) { appendTo.Append(','); }
                                        appendTo.AppendFormat(@"""{0}""", q.Question.ToXmlNodeName());
                                        appendComma = true;
                                    }
                                }
                            }
                            */
						} else {
							if(appendComma) { appendTo.Append(','); }
							appendTo.AppendFormat(@"""{0}""", p.Name);
							appendComma = true;
						}
					}
					appendTo.AppendLine();
				}

				appendComma = false;
				foreach(PropertyInfo p in t.GetProperties()) {
					if(appendComma) { appendTo.Append(','); }
					prop = t.GetProperty(p.Name);
					object[] attrs = prop.GetCustomAttributes(typeof(XmlIgnoreAttribute), false);
					if(attrs.Length >= 1) { continue; }

					value = prop.GetValue(obj);
					switch(Type.GetTypeCode(p.PropertyType)) {
						case TypeCode.String:
							if(value != null) { appendTo.AppendFormat(@"""{0}""", value.ToString().Trim()); }
							break;
						case TypeCode.DateTime:
							if(value != null) {
								DateTime? d = value as DateTime?;
								if(d.HasValue) { appendTo.AppendFormat(@"""{0}""", d.ToString()); }
							}
							break;
						case TypeCode.Object:
							/*
                            if (value != null) {
                                if (value is List<Questions>) {
                                    foreach (var q in value as List<Questions>) {
                                        if (appendComma) { appendTo.Append(','); }
                                        appendTo.AppendFormat(@"""{0}""", q.Answer.Trim());
                                        appendComma = true;
                                    }
                                }
                            }
                            */
							break;
						default:
							if(value != null) { appendTo.Append(value); }
							break;
					}
					appendComma = true;
				}
				appendTo.AppendLine();
			}

			return appendTo.ToString();
		}

		public static String SerializeXml<T>(this T obj) {
			XmlSerializer serializer = new XmlSerializer(typeof(T));

			StringBuilder xml = new StringBuilder();
			using(XmlWriter writer = XmlWriter.Create(xml)) {
				serializer.Serialize(writer, obj);
			};


			Match match = Regex.Match(xml.ToString(), @"\<\/(\w+)\>(\<\/\w+\>)$");
			if(match.Success) {
				xml.Insert(match.Groups[2].Index, string.Format("<{0} />", match.Groups[1].Value)).ToString();
			}
			return xml.ToString();
		}

		public static T DeserializeXml<T>(this String str) {
			XmlSerializer ser = new XmlSerializer(typeof(T));
			using(StringReader reader = new StringReader(str)) {
				return (T)ser.Deserialize(reader);
			};
		}

		public static string ToXmlNodeName(this string value) {
			return Regex.Replace(value.Trim().Replace(' ', '_'), @"[^a-zA-Z0-9_]", "");
		}

		public static string FormatPhone(this string value) {
			if(value.HasValue()) {
				Match match = Regex.Match(value, @"^(\d{3})(\d{3})(\d{4})$", RegexOptions.IgnoreCase);
				if(match.Success) {
					return string.Format("({0}) {1}-{2}", match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
				}
			}
			return value;
		}


		public static List<dynamic> ToExportView(this List<Customer> customers, LoginUser user) {
			return (from c in customers select c.ToExportView()).ToList();
		}

		public static dynamic ToExportView(this Customer customer) {
			return new {
				PatientId = customer.UserId,
				VerticalName = customer.VerticalName,
				FirstName = customer.FirstName ?? "",
				LastName = customer.LastName ?? "",
				Email = customer.Email ?? "",
				Phone = customer.Phone.FormatPhone() ?? "",
				Address = customer.Address ?? "",
				City = customer.City ?? "",
				State = customer.StateAbbr ?? "",
				ZipCode = customer.ZipCode ?? "",
				HealthInsurance = customer.HealthInsurance ?? "",
				ClientName = customer.CompanyName ?? "",
				ClientOffice = customer.OfficeName ?? "",
				DistanceToOffice = customer.Distance.Format(),
				Coordinator = customer.PatientCoordName ?? "",
				CreatedTime = customer.CreatedTime.ToString("M/d/yyyy h:mm tt"),
				UpdatedTime = customer.UpdatedTime.ToString("M/d/yyyy h:mm tt"),
				Notes = customer.Notes ?? "",
				ListOfQuestions = customer.ListOfQuestions.ToFlattenView()
			};
		}

		public static List<dynamic> ToFlattenView(this List<Question<QuestionInput>> list) {
			return (from q in list select q.ToFlattenView()).ToList();
		}

		public static dynamic ToFlattenView(this Question<QuestionInput> q) {
			string[] arry = (from a in q.ListOfInputs where a.Answer.HasValue() select a.Answer).ToArray();

			return new {
				Question = q.QuestionText.RexExReplace(@"^\d+\)\s+", string.Empty).Trim(),
				Answer = q.ListOfInputs.ToCommaDelimited() //string.Join(",", arry)
			};
		}

		public static string ToCommaDelimited(this List<QuestionInput> list) {
			if(list == null || list.Count == 0) { return string.Empty; }
			string[] arry = (from a in list where a.Answer.HasValue() select a.InsuranceName ?? a.Answer).ToArray();
			return string.Join(",", arry);
		}


		public static string GetValue(this HttpCookie cookie, string key, string defaultValue) {
			if(cookie == null) { return defaultValue; }
			return cookie[key] ?? defaultValue;
		}

		public static string GetRootDomain(this Uri uri) {
			if(uri.IsLocal()) { return null; }

			string retValue = uri.DnsSafeHost;
			string pattern;
			if(retValue.Matches(@"\.rmiatl\.[a-z]{2,4}$")) {
				pattern = @"^(?:[a-z0-9-]+\.)?([a-z0-9-.]+\.[a-z]+\.rmiatl\.[a-z]{2,4})$";
			} else {
				pattern = @"^(?:[a-z0-9-]+\.)?([a-z0-9-.]+\.[a-z]{2,4})$";
			}
			string[] groups;
			if(retValue.Matches(pattern, out groups)) {
				return groups[1];
			}
			return retValue;
		}

		public static string ToTitleCase(this string value) {
			if(value.IsEmpty()) { return value; }
			string lowerCase = value.ToLower().Trim();
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lowerCase);
		}

		/*public static string ToLowerCase(this string value) {
            if (value.IsEmpty()) { return value; }
            return value.ToLower();
        }*/

		public static D AutoFill<D>(this D destObject, object source) where D : class {
			if(source == null || destObject == null) { return null; }
			
			Type sourceType = source.GetType();
			Type destType = typeof(D);
			PropertyInfo[] properties = destType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			PropertyInfo sourceProp;
			string propName;
			object value;
			Type targetType;
			CloneAlias alias;
			foreach(PropertyInfo prop in properties) {
				alias = prop.GetCustomAttribute<CloneAlias>();
				propName = alias != null ? alias.Alias : prop.Name;
				if((sourceProp = sourceType.GetProperty(propName)) != null) {
					targetType = prop.PropertyType.IsNullableType() ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType;
					value = sourceProp.GetValue(source);
					try {
						if(value != null) {
							if(targetType.IsEnum) {
								value = Enum.ToObject(targetType, value);
							} else {
								value = Convert.ChangeType(value, targetType);
							}
						}
						prop.SetValue(destObject, value, null);
					} catch {

						//Ignore Error
					}
				}

			}
			return destObject;
		}

		private static PropertyInfo FindProperty(this Type type, string name) {
			return null;
		}

		public static D CloneAs<D>(this object source, params object[] args) where D : class {
			if(source == null) { return null; }

			Type sourceType = source.GetType();
			Type destType = typeof(D);
			Type gType;
			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

			object[] param = args.Length > 0 ? args : null;
			D destObject = (D)Activator.CreateInstance(destType, flags, null, param, null);

			PropertyInfo[] destProps = destType.GetProperties(flags);
			PropertyInfo sourceProp;
			string propName;
			object value;
			Type targetType;
			CloneAlias alias;
			var aliases = (from p in sourceType.GetProperties(flags)
						   let attr = p.GetCustomAttribute<CloneAlias>()
						   where attr != null
						   select new { prop = p, alias = attr.Alias }).ToList();

			foreach(PropertyInfo prop in destProps) {
				alias = prop.GetCustomAttribute<CloneAlias>();
				propName = alias != null ? alias.Alias : prop.Name;
				if(null == (sourceProp = sourceType.GetProperty(propName))) {
					var info = aliases.SingleOrDefault(p => p.alias == propName);
					if(info != null) {
						sourceProp = info.prop;
					} else {
						continue;
					}
				}
				if(sourceProp != null) {
					targetType = prop.PropertyType.IsNullableType() ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType;
					value = sourceProp.GetValue(source);
					try {
						if(value != null) {
							switch(Type.GetTypeCode(targetType)) {
								case TypeCode.Object:
									/*if (typeof(IEnumerable).IsAssignableFrom(targetType)) {
										gType = targetType.GenericTypeArguments.First();
										value = ((List<object>)value).Select(i => Convert.ChangeType(i, gType));
									}*/
									break;
								default:
									if (targetType.IsEnum) {
										value = Enum.ToObject(targetType, value);
									} else {
										value = Convert.ChangeType(value, targetType);
									}
									break;
							}
							prop.SetValue(destObject, value, null);
						}
					} catch(Exception ex) {
						//TODO: Add some functionality to convert List<Type>
						string.Format("");
					}
				}
			}
			return destObject;
		}

		private static bool IsNullableType(this Type type) {
			return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
		}

		private static object Parse(this string ValueToConvert, Type dataType) {
			try {
				TypeConverter obj = TypeDescriptor.GetConverter(dataType);
				object value = obj.ConvertFromString(null, CultureInfo.InvariantCulture, ValueToConvert);
				return value;
			} catch {
				return null;
			}
		}

		public static object Deserialize(this NameValueCollection value, Type type) {
			object retObject = Activator.CreateInstance(type);

			object propValue;
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
			PropertyInfo[] props = retObject.GetType().GetProperties(flags);
			PropertyInfo[] listProperties;
			IList listProp;
			object listItem;
			string propName;

			foreach(PropertyInfo p in props) {
				try {
					if(p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>)) {
						Type[] args = p.PropertyType.GetGenericArguments();
						Type genType = p.PropertyType.GetGenericTypeDefinition().MakeGenericType(args);
						listProp = Activator.CreateInstance(genType) as IList;

						listProperties = args[0].GetProperties(flags);
						while(value.AllKeys.Where(k => k.Matches(string.Format(@"^{0}\[{1}\]", p.Name, listProp.Count))).ToArray().Length > 0) {
							listItem = Activator.CreateInstance(args[0]);

							foreach(PropertyInfo prop in listProperties) {
								propName = string.Format("{0}[{1}].{2}", p.Name, listProp.Count, prop.Name);

								if((propValue = value[propName].Parse(prop.PropertyType)) != null) {
									prop.SetValue(listItem, propValue);
								}
							}
							listProp.Add(listItem);

						}
						p.SetValue(retObject, listProp);
					} else if((propValue = value[p.Name].Parse(p.PropertyType)) != null) {
						p.SetValue(retObject, propValue);
					}
				} catch(Exception ex) {
					ex.ToString();
				}

				/*
                switch (Type.GetTypeCode(p.PropertyType)) {
                    case TypeCode.Object:
                        if (p.PropertyType.IsArray) {
                            arryType = p.PropertyType.GetElementType();
                            arryProperties = arryType.GetProperties(flags);
                            arryList = new List<object>();
                            
                            while (value.AllKeys.Where(k => k.Matches(string.Format(@"^{0}\[{1}\]", p.Name, arryList.Count))).ToArray().Length > 0) {
                                arryObj = Activator.CreateInstance(assyName, arryType.FullName).Unwrap();
                                foreach (PropertyInfo arryProp in arryProperties) {
                                    arryPropName = string.Format("{0}[{1}].{2}", p.Name, arryList.Count, arryProp.Name);
                                    if ((propValue = value[arryPropName].Parse(arryProp.PropertyType)) != null) {
                                        arryProp.SetValue(arryObj, propValue);
                                    }
                                }
                                arryList.Add(arryObj);
                            }
                            if (arryList.Count > 0) {
                                Array arry = Array.CreateInstance(arryType, arryList.Count);
                                Array.Copy(arryList.ToArray(), arry, arryList.Count);
                                p.SetValue(retObject, arry);
                            }
                            System.Diagnostics.Debugger.Break();
                        } else if (p.PropertyType.IsGenericType) {
                            //var instance = p.PropertyType.MakeGenericType(p.PropertyType.GenericTypeArguments);
                            //var list = p.PropertyType.MakeGenericType(p.PropertyType);
                            var list = Activator.CreateInstance(p.PropertyType, p.PropertyType.GetGenericArguments());


                            System.Diagnostics.Debugger.Break();
                        } else {
                            throw new HttpUnhandledException();
                        }
                        break;
                    default:
                        if ((propValue = value[p.Name].Parse(p.PropertyType)) != null) {
                            p.SetValue(retObject, propValue);
                        }
                        break;
                }*/

			}
			return retObject;
		}

		public static T Deserialize<T>(this Stream stream) where T : class, new() {
			return stream.Deserialize(typeof(T)) as T;

		}

		public static object Deserialize(this Stream stream, Type type) {
			if(stream != null) {
				using(StreamReader rdr = new StreamReader(stream)) {
					return HttpUtility.ParseQueryString(rdr.ReadToEnd()).Deserialize(type);
				}

			}
			return null;
		}

		public static HttpRequestBase ToRequestBase(this HttpRequest request) {
			return new HttpRequestWrapper(request);
		}

		public static T GetColumnValue<T>(this SqlDataReader rdr, string colName, T defaultValue) {
			try {
				object value = rdr[colName];
				if(value is T) {
					return (T)value;

				} else if(value != null && value != DBNull.Value) {
					return (T)Convert.ChangeType(value, typeof(T));
				}
			} catch {
				//Ignore any errors
			}
			return defaultValue;
		}

		public static StringContent ToStringContent(this string content, string contentType) {
			return new StringContent(content, Encoding.UTF8, contentType);
		}

		public static StringContent ToJsonContent(this string content) {
			return content.ToStringContent("application/json");
		}

		public static void TrimStringProperties<T>(this T obj) {
			Type t = obj.GetType();
			var props = (from p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
						 where p.CanRead && p.CanWrite && Type.GetTypeCode(p.PropertyType).In(TypeCode.String, TypeCode.Object)
						 select p).ToList();
			object value;
			foreach(PropertyInfo prop in props) {
				value = prop.GetValue(obj);
				if(value != null) {
					switch(Type.GetTypeCode(prop.PropertyType)) {
						case TypeCode.String:
							prop.SetValue(obj, value.ToString().Trim());
							break;
						default:
							if(!prop.PropertyType.IsGenericType || prop.PropertyType.GetGenericTypeDefinition() != typeof(Nullable<>)) {
								if(value is IEnumerable) {
									foreach(object item in (IEnumerable)value) {
										item.TrimStringProperties();
									}
								} else {
									value.TrimStringProperties();
								}
							}
							break;
					}
				}
			}

		}
	}
}