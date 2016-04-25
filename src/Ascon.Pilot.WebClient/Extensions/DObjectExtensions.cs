using System.Linq;
using System.Text;
using Ascon.Pilot.Core;
using Ascon.Pilot.WebClient.Utils;

namespace Ascon.Pilot.WebClient.Extensions
{
    public static class DObjectExtensions
    {
        public static string GetTitle(this DObject obj, MType type)
        {
            /*if (type.IsProjectFileOrFolder())
            {
                DValue name;
                if (obj.Attributes.TryGetValue(SystemAttributes.PROJECT_ITEM_NAME, out name))
                    return name;
                return "unnamed";
            }*/
            return GetObjectTitle(obj, type);
        }

        private static string GetObjectTitle(DObject obj, MType type)
        {
            var sb = new StringBuilder();
            var attributes = AttributeFormatter.Format(type, obj.Attributes.ToDictionary(attr => attr.Key, attr => (attr.Value)));

            foreach (var displayableAttr in type.Attributes)
            {
                DValue value;
                if (attributes.TryGetValue(displayableAttr.Name, out value))
                {
                    var strValue = value.ToString();
                    if (sb.Length != 0)
                        sb.Append(Constants.PROJECT_TITLE_ATTRIBUTES_DELIMITER);

                    sb.Append(strValue);
                }
            }
            return sb.ToString();
        }
    }
}
