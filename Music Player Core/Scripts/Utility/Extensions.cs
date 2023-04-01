using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;


namespace Music_Player.Utility
{
    public static class Extensions
    {
        /// <summary>
        /// Clones object into new object, while cloning reference based objects into new ones
        /// </summary>
        /// <returns>Entire clone of this object and it's references</returns>
        public static T DeepClone<T>(this T objekt)
        {
            MemoryStream memoryStream = new MemoryStream();

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, objekt);
            memoryStream.Position = 0;

            return (T)binaryFormatter.Deserialize(memoryStream);
        }

        /// <summary>
        /// Clones value type objects, and assign any reference based field to it's cloned object
        /// </summary>
        /// <returns>Shallow part of this object</returns>
        public static T ShallowClone<T>(this T objekt)
        {
            object newObject = new object();

            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                typeof(T).GetProperty(propertyInfo.Name).SetValue(newObject, propertyInfo.GetValue(objekt));

            return (T)newObject;
        }
    }
}