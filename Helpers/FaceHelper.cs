using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientLibrary.Controls;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace ClientLibrary.Helpers
{
    public class FaceHelper
    {
        private FaceServiceClient faceServiceClient;
        private DBHelper database;

        private static Guid BASE_GROUP_ID = new Guid("7DA0173D-DF74-4B73-A2F0-9880130A7068");
        private static Guid BRAM_PERSON_ID = new Guid("f679205d-64b4-445b-99cf-9a08f87fb509");
        //private static Guid BRAM_PERSON_ID = new Guid("C3A70A67-DB64-49FD-B064-B32AFA8CE278");
        private static string BRAM_PERSON_NAME = "BRAMMEL";
        private static string BASE_GROUP_NAME = "BASE_GROUP";
        private static Guid BRAM_FACE_LIST_ID = new Guid("11EF5FB3-C3C5-40CE-B464-B43508C9C698");
        private static string BRAM_FACE_LIST_NAME = "BRAM_FACE_LIST";

        public FaceHelper()
        {
            string subscriptionKey = Constants.FACE_SUBSCRIPTION_KEY;
            faceServiceClient = new FaceServiceClient(subscriptionKey);
            database = new DBHelper();

            var face1 = database.GetFaceListFromFaceId("7e24a58b-784a-42c1-b16b-b6b655b35b63");
            var faces = database.GetFaces().Where(f=>f.FaceId.ToString() == "7e24a58b-784a-42c1-b16b-b6b655b35b63").ToList();

            CreateBram();

            
        }

        public void AddBramFacePerson(Stream imageFileStream)
        {
            try
            {
                var result = Task.Run(() => faceServiceClient.AddPersonFaceAsync(BASE_GROUP_ID.ToString(), BRAM_PERSON_ID, imageFileStream)).Result;
            }
            catch (Exception e)
            {
                
            }
        }
        public void AddBramFaceList(Stream imageFileStream, string imageFileName)
        {
            try
            {
                AddPersistedFaceResult result = Task.Run(() => faceServiceClient.AddFaceToFaceListAsync(BRAM_FACE_LIST_ID.ToString(), imageFileStream)).Result;
                database.InsertFace(result.PersistedFaceId.ToString(), BRAM_FACE_LIST_ID.ToString(), imageFileName);
            }
            catch (Exception e)
            {
                
            }
        }

        //try detection face from the image supplied in a list
        public string DetectFace(string fullImagePath)
        {
            using (var fileStream = new FileStream(fullImagePath, FileMode.Open))
            {
                Face[] faceDetectResult = Task.Run(() => faceServiceClient.DetectAsync(fileStream, true, false)).Result;
                foreach (Face face in faceDetectResult)
                {
                    //database.InsertFace(face.FaceId.ToString(), null, fullImagePath);

                    SimilarPersistedFace[] similarFacesResult = Task.Run(() => faceServiceClient.FindSimilarAsync(face.FaceId, BRAM_FACE_LIST_ID.ToString(), FindSimilarMatchMode.matchPerson, 1000)).Result;
                    SimilarPersistedFace bestMachingResult = similarFacesResult.FirstOrDefault();
                    if (bestMachingResult != null)
                    {
                        FaceList matchingPersonList = database.GetFaceListFromFaceId(bestMachingResult.PersistedFaceId.ToString());
                        if (matchingPersonList != null)
                        {
                            return matchingPersonList.Name;
                        }
                        else
                        {
                            
                        }
                        //matchingPersonList.Name
                    }
                }
            }
            return null;
        }
        public void TEST()
        {
            //AddBramImagesTEST();
//            database.InsertFace(Guid.NewGuid().ToString(), BRAM_FACE_LIST_ID.ToString(), "fadsadsffdsafdsa.jpg");
            var faces = database.GetFaces();
        }
        public void AddBramImagesTEST()
        {
            string lifecamPath = @"d:\_Data_\Bram\_Projecten_\MSFaceCognitiveServiceTest\_Pics_\";
            foreach (string file in Directory.EnumerateFiles(lifecamPath, "*.jpg"))
            {
                string fullPath = Path.Combine(lifecamPath, file);
                using (var fileStream = new FileStream(fullPath, FileMode.OpenOrCreate))
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    AddBramFaceList(fileStream, file);
                }
            }
        }

        private void DeleteBram()
        {
            
        }
        private void CreateBram()
        {
            //person is quite useless, the recognition itself is sone with faceLists
//            bool bramExists = false;
//            //checks if bram exists
//            try
//            {
//                var allPersons = Task.Run(() => faceServiceClient.GetPersonsAsync(BASE_GROUP_ID.ToString())).Result;
//                
//                Person bramPerson = allPersons.FirstOrDefault(p => p.Name.Equals(BRAM_PERSON_NAME, StringComparison.OrdinalIgnoreCase)); //Task.Run(() => faceServiceClient.GetPersonAsync(BASE_GROUP_ID.ToString(), BRAM_PERSON_ID)).Result;
//                bramExists = bramPerson != null;
//
//                if (bramExists)
//                {
//                    BRAM_PERSON_ID = bramPerson.PersonId;
//                }
//            }
//            catch (Exception e)
//            {
//                bramExists = false;
//            }
//
//            if (bramExists == false)
//            {
//                CreateBaseGroup();
//
//                //creates dem bram
//                CreatePersonResult bramPersonCreationResult = Task.Run(() => faceServiceClient.CreatePersonAsync(BASE_GROUP_ID.ToString(), BRAM_PERSON_NAME, null)).Result;
//                BRAM_PERSON_ID = bramPersonCreationResult.PersonId;
//            }

            //bram face list
            bool bramFaceListExists = false;
            FaceList bramFaceList;
            try
            {
                bramFaceList = Task.Run(() => faceServiceClient.GetFaceListAsync(BRAM_FACE_LIST_ID.ToString())).Result;
                bramFaceListExists = bramFaceList != null;
            }
            catch (Exception e)
            {
                bramFaceListExists = false;
            }
            if (bramFaceListExists == false)
            {
                Task.Run(() => faceServiceClient.CreateFaceListAsync(BRAM_FACE_LIST_ID.ToString(), BRAM_FACE_LIST_NAME, null));
            }

            //if the face list is not in de DB, adds it
            bramFaceList = Task.Run(() => faceServiceClient.GetFaceListAsync(BRAM_FACE_LIST_ID.ToString())).Result;
            if (bramFaceList != null && database.GetFaceListById(bramFaceList.FaceListId) == null)
            {
                database.InsertFaceList(bramFaceList.FaceListId, bramFaceList.Name);
            }
                
        }

        
        private void CreateBaseGroup()
        {
            bool baseGroupExists = false;
            //checks if the base group exists
            try
            {
                PersonGroup basePersonGroup = Task.Run(() => faceServiceClient.GetPersonGroupAsync(BASE_GROUP_ID.ToString())).Result;
                baseGroupExists = basePersonGroup != null;
            }
            catch (Exception e)
            {
                baseGroupExists = false;
            }

            //if the base group doesnt exists, creates it
            if (baseGroupExists == false)
            {
                Task.Run(() => faceServiceClient.CreatePersonGroupAsync(BASE_GROUP_ID.ToString(), BASE_GROUP_NAME));
            }
        }

        private void DeleteBramFaceList()
        {
            Task.Run(() => faceServiceClient.DeleteFaceListAsync(BRAM_FACE_LIST_ID.ToString()));
        }
    }
}
