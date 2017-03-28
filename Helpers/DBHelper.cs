using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClientLibrary.Controls;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face.Model;

namespace ClientLibrary.Helpers
{
    public class DBHelper
    {
        private SQLiteConnection connection;

        public DBHelper()
        {
            OpenDBConnection();

            CreateTables();
        }

        private void OpenDBConnection()
        {
            string fullPath = Path.Combine(@"d:\_Data_\Bram\_Projecten_\MSFaceCognitiveServiceTest\_DB_\", "MSCognitive.db");
            if (File.Exists(fullPath) == false)
            {
                SQLiteConnection.CreateFile(fullPath);
            }

            connection = new SQLiteConnection($"Data Source={fullPath};Version=3;");
            connection.Open();
        }

        private void CreateTables()
        {
            string query = $"create table if not exists {Constants.DB_PERSON_TABLE_NAME} (id INTEGER PRIMARY KEY AUTOINCREMENT, {Constants.DB_PERSON_PERSON_ID_FIELDNAME} varchar(36), {Constants.DB_PERSON_PERSON_NAME_FIELDNAME} varchar(100), {Constants.DB_PERSON_GROUP_ID_FIELDNAME} varchar(36))";
            SQLiteCommand command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();

            query = $"create table if not exists '{Constants.DB_GROUP_TABLE_NAME}' (id INTEGER PRIMARY KEY AUTOINCREMENT, '{Constants.DB_GROUP_GROUP_ID_FIELDNAME }' varchar(36), {Constants.DB_GROUP_GROUP_NAME_FIELDNAME} varchar(100) )";
            command= new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();

            query = $"create table if not exists {Constants.DB_LIST_TABLE_NAME} (id INTEGER PRIMARY KEY AUTOINCREMENT, {Constants.DB_LIST_ID_FIELDNAME } varchar(36), {Constants.DB_LIST_NAME_FIELDNAME} varchar(100) )";
            command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();

            query = $"create table if not exists {Constants.DB_FACE_TABLE_NAME} (id INTEGER PRIMARY KEY AUTOINCREMENT, {Constants.DB_FACE_ID_FIELDNAME } varchar(36), {Constants.DB_FACE_FACE_LIST_ID_FIELDNAME} varchar(36), {Constants.DB_FACE_IMAGENAME_FIELDNAME} varchar(100), {Constants.DB_FACE_PERSON_ID_FIELDNAME} varchar(36) )";
            command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();
    }

        public void InsertPerson(string personId, string name, string groupId)
        {
            string sql = $"insert into {Constants.DB_PERSON_TABLE_NAME} ({Constants.DB_PERSON_PERSON_ID_FIELDNAME}, {Constants.DB_PERSON_PERSON_NAME_FIELDNAME}, {Constants.DB_PERSON_GROUP_ID_FIELDNAME}) values ('{personId}', '{name}', '{groupId}' )";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }
        public void InsertGroup(string groupId, string name)
        {
            string sql = $"insert into '{Constants.DB_GROUP_TABLE_NAME}' ({Constants.DB_GROUP_GROUP_ID_FIELDNAME}, {Constants.DB_GROUP_GROUP_NAME_FIELDNAME}) values ('{groupId}', '{name}')";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }
        public void InsertFaceList(string faceListId, string name)
        {
            string sql = $"insert into {Constants.DB_LIST_TABLE_NAME} ({Constants.DB_LIST_ID_FIELDNAME}, {Constants.DB_LIST_NAME_FIELDNAME}) values ('{faceListId}', '{name}')";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }
        public void InsertFace(string persistedFaceId, string faceListId, string imageName, string personId)
        {
            string sql = $"insert into {Constants.DB_FACE_TABLE_NAME} ({Constants.DB_FACE_ID_FIELDNAME}, {Constants.DB_FACE_FACE_LIST_ID_FIELDNAME}, {Constants.DB_FACE_IMAGENAME_FIELDNAME}, {Constants.DB_FACE_PERSON_ID_FIELDNAME}) values ('{persistedFaceId}', '{faceListId}', '{imageName}', '{personId}')";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public PersonExtended GetPerson(string personId)
        {
            string sql = $"select * from {Constants.DB_PERSON_TABLE_NAME} WHERE {Constants.DB_PERSON_PERSON_ID_FIELDNAME } = '{personId}' limit 1";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return new PersonExtended() { Name = reader[Constants.DB_PERSON_PERSON_NAME_FIELDNAME].ToString(), PersonId = new Guid(reader[Constants.DB_PERSON_PERSON_ID_FIELDNAME].ToString()), GroupId = IsGuid(reader[Constants.DB_PERSON_GROUP_ID_FIELDNAME].ToString()) ?  new Guid(reader[Constants.DB_PERSON_GROUP_ID_FIELDNAME].ToString()) : Guid.Empty };
            }
            return null;
        }
        public List<PersonExtended> GetPersons()
        {
            List<PersonExtended> result = new List<PersonExtended>();
            string sql = $"select * from {Constants.DB_PERSON_TABLE_NAME}";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PersonExtended() { Name = reader[Constants.DB_PERSON_PERSON_NAME_FIELDNAME].ToString(), PersonId = new Guid(reader[Constants.DB_PERSON_PERSON_ID_FIELDNAME].ToString()), GroupId = IsGuid(reader[Constants.DB_PERSON_GROUP_ID_FIELDNAME].ToString()) ? new Guid(reader[Constants.DB_PERSON_GROUP_ID_FIELDNAME].ToString()) : Guid.Empty });
            }
            return result;
        }
        public PersonExtended GetPersonByName(string personName)
        {
            string sql = $"select * from {Constants.DB_PERSON_TABLE_NAME} WHERE {Constants.DB_PERSON_PERSON_NAME_FIELDNAME} = '{personName}' limit 1";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return new PersonExtended()
                {
                    Name = reader[Constants.DB_PERSON_PERSON_NAME_FIELDNAME].ToString(),
                    PersonId = IsGuid(reader[Constants.DB_PERSON_PERSON_ID_FIELDNAME].ToString()) ? new Guid(reader[Constants.DB_PERSON_PERSON_ID_FIELDNAME].ToString()) : Guid.Empty,
                    GroupId = IsGuid(reader[Constants.DB_PERSON_GROUP_ID_FIELDNAME].ToString()) ? new Guid(reader[Constants.DB_PERSON_GROUP_ID_FIELDNAME].ToString()) : Guid.Empty
                };
            }
            return null;
        }
        public FaceList GetFaceList(string name)
        {
            string sql = $"select * from {Constants.DB_LIST_TABLE_NAME} WHERE {Constants.DB_LIST_NAME_FIELDNAME } = '{name}' limit 1";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return new FaceList() { Name = reader[Constants.DB_LIST_ID_FIELDNAME].ToString(), FaceListId = reader[Constants.DB_LIST_ID_FIELDNAME].ToString() };
            }
            return null;
        }
        public FaceList GetFaceListById(string faceListId)
        {
            string sql = $"select * from {Constants.DB_LIST_TABLE_NAME} WHERE {Constants.DB_LIST_ID_FIELDNAME} = '{faceListId}' limit 1";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return new FaceList() { Name = reader[Constants.DB_LIST_ID_FIELDNAME].ToString(), FaceListId = reader[Constants.DB_LIST_ID_FIELDNAME].ToString() };
            }
            return null;
        }
        public List<FaceList> GetFaceLists()
        {
            List<FaceList> result = new List<FaceList>();

            string sql = $"select * from {Constants.DB_LIST_TABLE_NAME}";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new FaceList() { Name = reader[Constants.DB_LIST_ID_FIELDNAME].ToString(), FaceListId = reader[Constants.DB_LIST_ID_FIELDNAME].ToString() });
            }
            return result;
        }
        public FaceList GetFaceListFromFaceId(string faceId)
        {
            string sql = $"select fl.{Constants.DB_LIST_ID_FIELDNAME}, fl.{Constants.DB_LIST_NAME_FIELDNAME} from {Constants.DB_LIST_TABLE_NAME} fl LEFT JOIN {Constants.DB_FACE_TABLE_NAME} as f ON fl.{Constants.DB_LIST_ID_FIELDNAME} = f.{Constants.DB_FACE_FACE_LIST_ID_FIELDNAME} WHERE f.{Constants.DB_FACE_ID_FIELDNAME } = '{faceId}' limit 1";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return new FaceList() { Name = reader[Constants.DB_LIST_NAME_FIELDNAME].ToString(), FaceListId = reader[Constants.DB_LIST_ID_FIELDNAME].ToString() };
            }
            return null;
        }
        public PersonExtended GetPersonFromFaceId(string faceId)
        {
            string sql = $"select p.{Constants.DB_PERSON_PERSON_ID_FIELDNAME }, p.{Constants.DB_PERSON_PERSON_NAME_FIELDNAME}, p.{Constants.DB_PERSON_GROUP_ID_FIELDNAME } from {Constants.DB_PERSON_TABLE_NAME} p LEFT JOIN {Constants.DB_FACE_TABLE_NAME} as f ON f.{Constants.DB_FACE_PERSON_ID_FIELDNAME} = p.{Constants.DB_PERSON_PERSON_ID_FIELDNAME} WHERE f.{Constants.DB_FACE_ID_FIELDNAME} = '{faceId}' limit 1";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                return new PersonExtended() {PersonId = new Guid(reader[Constants.DB_PERSON_PERSON_ID_FIELDNAME].ToString()), Name = reader[Constants.DB_PERSON_PERSON_NAME_FIELDNAME].ToString(), GroupId = (reader[Constants.DB_PERSON_GROUP_ID_FIELDNAME] != null && string.IsNullOrEmpty(reader[Constants.DB_PERSON_GROUP_ID_FIELDNAME].ToString()) == false) ? new Guid(reader[Constants.DB_PERSON_GROUP_ID_FIELDNAME].ToString()) : Guid.Empty };
            }
            return null;
        }
        public List<Face> GetFacesFromPersonName(string personName)
        {
            List<Face> result = new List<Face>();
            string sql = $"select f.{Constants.DB_FACE_ID_FIELDNAME }, f.{Constants.DB_FACE_IMAGENAME_FIELDNAME}, f.{Constants.DB_FACE_PERSON_ID_FIELDNAME }, f.{Constants.DB_FACE_FACE_LIST_ID_FIELDNAME } from {Constants.DB_FACE_TABLE_NAME} f LEFT JOIN {Constants.DB_PERSON_TABLE_NAME} as p ON f.{Constants.DB_FACE_PERSON_ID_FIELDNAME} = p.{Constants.DB_PERSON_PERSON_ID_FIELDNAME} WHERE p.{Constants.DB_PERSON_PERSON_NAME_FIELDNAME} = '{personName}'";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new FaceExtended()
                {
                    FaceId = IsGuid(reader[Constants.DB_FACE_ID_FIELDNAME].ToString()) ? new Guid(reader[Constants.DB_FACE_ID_FIELDNAME].ToString()) : Guid.Empty,
                    FaceListId = IsGuid(reader[Constants.DB_FACE_FACE_LIST_ID_FIELDNAME].ToString()) ? new Guid(reader[Constants.DB_FACE_FACE_LIST_ID_FIELDNAME].ToString()) : Guid.Empty,
                    ImageName = reader[Constants.DB_FACE_IMAGENAME_FIELDNAME].ToString(),
                    PersonId = IsGuid(reader[Constants.DB_FACE_PERSON_ID_FIELDNAME].ToString()) ?  new Guid(reader[Constants.DB_FACE_PERSON_ID_FIELDNAME].ToString()) : Guid.Empty,
                });
            }
            return result;
        }
        public List<PersonExtended> SearchPersons(string personName)
        {
            List<PersonExtended> result = new List<PersonExtended>();

            string sql = $"select * from {Constants.DB_PERSON_TABLE_NAME} WHERE {Constants.DB_PERSON_PERSON_NAME_FIELDNAME} = '{personName}'"; 
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PersonExtended()
                {
                    Name = reader[Constants.DB_PERSON_PERSON_NAME_FIELDNAME].ToString(),
                    PersonId = new Guid(reader[Constants.DB_PERSON_PERSON_ID_FIELDNAME].ToString()),
                    GroupId = new Guid(reader[Constants.DB_PERSON_GROUP_ID_FIELDNAME].ToString())
                });
            }
            return result;
        }

        public List<FaceExtended> GetFaces(string faceListId = null)
        {
            List<FaceExtended> result = new List<FaceExtended>();

            string sql = $"select * from {Constants.DB_FACE_TABLE_NAME}";
            if (faceListId != null)
            {
                sql += $" WHERE {Constants.DB_FACE_FACE_LIST_ID_FIELDNAME} = '{faceListId}'";
            }
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new FaceExtended()
                {
                    FaceId = new Guid(reader[Constants.DB_FACE_ID_FIELDNAME].ToString()),
                    FaceListId = new Guid(reader[Constants.DB_FACE_FACE_LIST_ID_FIELDNAME].ToString()),
                    ImageName = reader[Constants.DB_FACE_IMAGENAME_FIELDNAME].ToString()
                });
            }
            return result;
        }

        private bool IsGuid(string guidStr)
        {
            try
            {
                Guid tmpGuid = new Guid(guidStr);
                return true;
            }
            catch (Exception e)
            {
                
            }
            return false;
        }
    }
}
