using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TheTrainer.PredictionMachines;

namespace TheTrainer.Helpers
{
    public class SerializationHelper
    {
        public static byte[] SerializeBot(PredictionMachine bot)
        {
            var serializer = new BinaryFormatter();
            byte[] serialized;
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, bot);
                ms.Position = 0;
                serialized = ms.ToArray();
            }
            return serialized;
        }

        public static PredictionMachine DeserializeBot(byte[] serializedBot)
        {
            PredictionMachine newBot;
            var serializer = new BinaryFormatter();
            using (var ms2 = new MemoryStream(serializedBot))
            {
                ms2.Position = 0;
                newBot = (PredictionMachine)serializer.Deserialize(ms2);
            }

            return newBot;
        }
    }
}