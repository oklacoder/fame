using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace fame.Persist.Postgresql
{
    public static class Util
    {
        public static IEnumerable<Type> GetAllLoadedCommandTypes()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implements =
                allAssemblies
                    .SelectMany(p =>
                    {
                        try
                        {
                            return p.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(x => x != null);
                        }
                    })
                    .Where(p => typeof(BaseCommand).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            return
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(BaseCommand).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();
        }
        public static IEnumerable<Type> GetAllLoadedEventTypes()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implements =
                allAssemblies
                    .SelectMany(p =>
                    {
                        try
                        {
                            return p.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(x => x != null);
                        }
                    })
                    .Where(p => typeof(BaseEvent).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            return
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(BaseEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();
        }
        public static IEnumerable<Type> GetAllLoadedQueryTypes()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implements =
                allAssemblies
                    .SelectMany(p =>
                    {
                        try
                        {
                            return p.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(x => x != null);
                        }
                    })
                    .Where(p => typeof(BaseQuery).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            return
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(BaseQuery).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();
        }
        public static IEnumerable<Type> GetAllLoadedResponseTypes()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implements =
                allAssemblies
                    .SelectMany(p =>
                    {
                        try
                        {
                            return p.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(x => x != null);
                        }
                    })
                    .Where(p => typeof(BaseResponse).IsAssignableFrom(p))
                    .Select(p => p.Assembly);

            return
                implements
                    .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(BaseResponse).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                    .Distinct()
                    .ToList();
        }
    }

}
