using System.ComponentModel.DataAnnotations;

namespace CroweAlumniPortal.Helper
{
    public static class EnumHelper
    {
        public static IEnumerable<object> ToDataset<TEnum>() where TEnum : Enum
        {
            var type = typeof(TEnum);
            foreach (var val in Enum.GetValues(type))
            {
                var name = Enum.GetName(type, val);
                var mem = type.GetMember(name)[0];
                var disp = mem.GetCustomAttributes(typeof(DisplayAttribute), false)
                              .Cast<DisplayAttribute>()
                              .FirstOrDefault()?.Name ?? name;

                yield return new
                {
                    value = (int)val,
                    key = name,  
                    label = disp   
                };
            }
        }
    }

}
