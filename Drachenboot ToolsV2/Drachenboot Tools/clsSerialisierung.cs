using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace Drachenboot_Tools
{
    [Serializable]
    public class clsSerialisierung
    {
        public int AnzTeams;
        public clsPaddler Paddler = new clsPaddler();
        public clsSerialisierungsSpeicher Speicher = new clsSerialisierungsSpeicher();
        
        
        #region Serialisierung
        /// <summary>
        /// Speichert alle Einstellungen in die Datei AppPath/Save.xml.
        /// </summary>
        public static void Serialize(clsSerialisierung settings)
        {
            var ser = new XmlSerializer(typeof(clsSerialisierung));

            TextWriter textWriter = new StreamWriter(clsConst.Save);
            ser.Serialize(textWriter, settings);
            textWriter.Close();
        }

        /// <summary>
        /// Läd alle Einstellungen aus der Save-Datei in eine Serialisierungs-Klasse.
        /// </summary>
        /// <returns>Eine Serialisierungs-Klasse mit den Einstellungen aus der Save.xml</returns>
        public static clsSerialisierung DeSerialize()
        {
            var deser = new XmlSerializer(typeof(clsSerialisierung));
            try
            {
                TextReader textReader = new StreamReader(clsConst.Save); 
                var returnValue = (clsSerialisierung)deser.Deserialize(textReader);
                textReader.Close();
                return returnValue;
            }
            catch (FileNotFoundException)
            {
                var ser = new XmlSerializer(typeof(clsSerialisierung));
                var x = new clsSerialisierung();
                TextWriter textWriter = new StreamWriter(clsConst.Save);
                ser.Serialize(textWriter, x);
                textWriter.Close();
                TextReader textReader = new StreamReader(clsConst.Save);
                var returnValue = (clsSerialisierung)deser.Deserialize(textReader);
                textReader.Close();
                return returnValue;
            }
            

            
        }
        #endregion
    }
}
