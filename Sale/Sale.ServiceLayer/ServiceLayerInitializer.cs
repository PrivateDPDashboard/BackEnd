using System.Collections.Generic;
using AutoMapper;
using CoffeeCode.DataBase.Base.Repository;
using CoffeeCode.ServiceLayer.Base;
using Microsoft.Extensions.DependencyInjection;
using Sale.ServiceLayer.MappingProfiles;

namespace Sale.ServiceLayer
{
    public class ServiceLayerInitializer
    {
        public static IMapper Initialize(IServiceCollection serviceCollection) {
            var configuration = new MapperConfiguration(e => {
                var list = new List<Profile>
                {
                    new BasicProfile(),
                    new CustomProfile()
                };
                e.AddProfiles(list);
            });

            var mapper = configuration.CreateMapper();

            serviceCollection.AddSingleton(mapper);

            //serviceCollection.AddIInjectableDependencies(typeof(LanguageRepository));
            serviceCollection.AddTransient(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));

            //serviceCollection.AddIInjectableDependencies(typeof(LanguageService));
            serviceCollection.AddTransient(typeof(IBaseService<,,>), typeof(BaseService<,,>));
            return mapper;
        }
    }
}
