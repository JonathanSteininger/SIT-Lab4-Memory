using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace forms
{
    public class User : DTO, IComparable<User>
    {
        public string UserName { get; set; }
        public int Correct { get; set; }
        public int Attempts { get; set; }

        public User() { 
            Type = nameof(User);
        }

        public User(string username, int correct, int attempst) : this()
        {
            this.UserName = username;
            this.Correct = correct;
            this.Attempts = attempst;
        }

        public override string ToString()
        {
            return $"{UserName}\tCorrect:{Correct}\tGuesses:{Attempts}";
        }

        public int CompareTo(User other)
        {
            int a = other.Correct.CompareTo(Correct);
            if (a != 0) return a;
            return Attempts.CompareTo(other.Attempts);
        }
    }
    

    public abstract class DTO
    {
        public string Type { get; set; }
    }

    public static class JsonFactory{

        public static string Serialize(object obj) => JsonConvert.SerializeObject(obj);
        public static T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);
            
        
        public static void Save(string filePath, object obj)
        {
            CreateDirectory(filePath);
            using(FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                BinaryWriter bw = new BinaryWriter(stream);
                bw.Write(Serialize(obj));
            }
        }
        public static T Read<T>(string filePath)
        {
            if(!File.Exists(filePath)) return default(T);
            using(FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader br = new BinaryReader(stream);
                return Deserialize<T>(br.ReadString());
            }
        }

        private static void CreateDirectory(string filePath)
        {
            if (File.Exists(filePath)) return;
            string[] parts = filePath.Split(new char[] {'/'});
            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < parts.Length - 1; i++)
            {
                sb.Append(parts[i]);
                sb.Append('/');
            }
            string path = sb.ToString();
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }
    }
}
