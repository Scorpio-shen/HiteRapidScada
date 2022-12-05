namespace KpCommon
{
    //public static class DIServiceHelper
    //{
    //    public static IContainer Container;
    //    public static ContainerBuilder containerBuilder;
    //    static DIServiceHelper()
    //    {
    //        containerBuilder = new ContainerBuilder();
    //    }

    //    public static void BulidService()
    //    {
    //        Container = containerBuilder.Build();   
    //    }

    //    public static ContainerBuilder RegisterType<T>(RegisterTypeEnum typeEnum = RegisterTypeEnum.Transient)
    //    {
    //        switch (typeEnum)
    //        {
    //            case RegisterTypeEnum.Transient:
    //                containerBuilder.RegisterType<T>().InstancePerDependency();
    //                break;
    //            case RegisterTypeEnum.Scope:
    //                containerBuilder.RegisterType<T>().InstancePerLifetimeScope();
    //                break;
    //            case RegisterTypeEnum.Single:
    //                containerBuilder.RegisterType<T>().SingleInstance();
    //                break;
    //        }

    //        return containerBuilder;
    //    }

    //    public static ContainerBuilder RegisterType<T>(string serviceName, RegisterTypeEnum typeEnum = RegisterTypeEnum.Transient)
    //    {
    //        switch (typeEnum)
    //        {
    //            case RegisterTypeEnum.Transient:
    //                containerBuilder.RegisterType<T>().InstancePerDependency().Named<T>(serviceName);
    //                //containerBuilder.RegisterType<T>().InstancePerDependency();
    //                break;
    //            case RegisterTypeEnum.Scope:
    //                containerBuilder.RegisterType<T>().InstancePerLifetimeScope().Named<T>(serviceName);
    //                break;
    //            case RegisterTypeEnum.Single:
    //                containerBuilder.RegisterType<T>().SingleInstance().Named<T>(serviceName);
    //                break;
    //        }

    //        return containerBuilder;
    //    }
    //}

    ///// <summary>
    ///// 注入三种方式
    ///// </summary>
    //public enum RegisterTypeEnum
    //{
    //    [Description("单例")]
    //    Single,
    //    [Description("作用域")]
    //    Scope,
    //    [Description("瞬时")]
    //    Transient
    //}
}
