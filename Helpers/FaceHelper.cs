using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientLibrary.Controls;
using ClientLibrary.Model;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face.Model;

namespace ClientLibrary.Helpers
{
    public class FaceHelper
    {
        private FaceServiceClient faceServiceClient;
        public DBHelper database;

        private static Guid BASE_GROUP_ID = new Guid("7DA0173D-DF74-4B73-A2F0-9880130A7068");
        private static Guid BRAM_PERSON_ID = new Guid("f679205d-64b4-445b-99cf-9a08f87fb509");
        //private static Guid BRAM_PERSON_ID = new Guid("C3A70A67-DB64-49FD-B064-B32AFA8CE278");
        private static string BRAM_PERSON_NAME = "BRAMMEL";
        private static string BASE_GROUP_NAME = "BASE_GROUP";
        private static Guid TMP_FACE_LIST = new Guid("11EF5FB3-C3C5-40CE-B464-B43508C9C698");
        private static string TMP_FACE_LIST_NAME = "TMP_FACE_LIST";

        public FaceHelper()
        {
            string subscriptionKey = Constants.FACE_SUBSCRIPTION_KEY;
            faceServiceClient = new FaceServiceClient(subscriptionKey);
            database = new DBHelper();

//            var face1 = database.GetFaceListFromFaceId("7e24a58b-784a-42c1-b16b-b6b655b35b63");
//            var faces = database.GetFaces().Where(f=>f.FaceId.ToString() == "7e24a58b-784a-42c1-b16b-b6b655b35b63").ToList();

            //DeleteTmpFaceList();
            CreateFaceList();
            
            //CreateBram();

            
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
        public string AddFaceToList(Stream imageFileStream)
        {
            try
            {
                AddPersistedFaceResult result = Task.Run(() => faceServiceClient.AddFaceToFaceListAsync(TMP_FACE_LIST.ToString(), imageFileStream)).Result;
                if (result != null)
                    return result.PersistedFaceId.ToString();
            }
            catch (Exception e)
            {
                
            }
            return null;
        }

        //try detection face from the image supplied in a list at the servers of microsoft
        //we got an local database with the same images and ids, thats how you know who it is
        public string DetectFace(string fullImagePath)
        {
            using (var fileStream = new FileStream(fullImagePath, FileMode.Open))
            {
                Face[] faceDetectResult = Task.Run(() => faceServiceClient.DetectAsync(fileStream, true, false)).Result;
                foreach (Face face in faceDetectResult)
                {
                    //database.InsertFace(face.FaceId.ToString(), null, fullImagePath);
                    try
                    {
                        SimilarPersistedFace[] similarFacesResult = Task.Run(() => faceServiceClient.FindSimilarAsync(face.FaceId, TMP_FACE_LIST.ToString(), FindSimilarMatchMode.matchPerson, 1000)).Result;
                        SimilarPersistedFace bestMachingResult = similarFacesResult.FirstOrDefault();
                        if (bestMachingResult != null)
                        {
                            //FaceList matchingPersonList = database.GetFaceListFromFaceId(bestMachingResult.PersistedFaceId.ToString());
                            PersonExtended matchingPerson = database.GetPersonFromFaceId(bestMachingResult.PersistedFaceId.ToString());
                            if (matchingPerson != null)
                            {
                                return matchingPerson.Name;
                            }
                            else
                            {

                            }
                            //matchingPersonList.Name
                        }
                    }
                    catch (AggregateException ex)
                    {
                        if (ex.InnerException is Microsoft.ProjectOxford.Face.FaceAPIException)
                        {
                            Microsoft.ProjectOxford.Face.FaceAPIException innerEx = ex.InnerException as Microsoft.ProjectOxford.Face.FaceAPIException;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    
                }
            }
            return null;
        }

        //adds all the images to the image list from microsoft
        //saves it local in the DB with the person its from
        public int AddPerson(string personName, List<ImageItem> images)
        {
            int amountUpdated = 0;
            PersonExtended person = database.GetPersonByName(personName);
            string personId = string.Empty;
            if (person == null)
            {
                personId = Guid.NewGuid().ToString();
                database.InsertPerson(personId, personName, "");
            }
            else
            {
                personId = person.PersonId.ToString();
            }

            foreach (ImageItem image in images)
            {
                using (var fileStream = new FileStream(image.FullPath, FileMode.OpenOrCreate))
                {
                    string newPath = Path.Combine(Constants.PERSONS_IMAGE_DIR, image.FileName);
                    fileStream.Seek(0, SeekOrigin.Begin);
                    //if the image isnt already added to the list
                    if (File.Exists(newPath) == false)
                    {
                        string newFaceId = AddFaceToList(fileStream); //returns null if failled
                        if (newFaceId != null)
                        {
                            string newFileName = newFaceId + ".jpg";
                            newPath = Path.Combine(Constants.PERSONS_IMAGE_DIR, newFileName);
                            //adds to local DB
                            database.InsertFace(newFaceId, TMP_FACE_LIST.ToString(), newFileName , personId);
                            File.Copy(image.FullPath, newPath);
                            amountUpdated++;
                        }
                        
                    }
                }
            }

            return amountUpdated;
        }
        public void TEST()
        {
            //AddBramImagesTEST();
//            database.InsertFace(Guid.NewGuid().ToString(), TMP_FACE_LIST.ToString(), "fadsadsffdsafdsa.jpg");
            var faces = database.GetFaces();
        }
//        public void AddBramImagesTEST()
//        {
//            string lifecamPath = @"d:\_Data_\Bram\_Projecten_\MSFaceCognitiveServiceTest\_Pics_\";
//            foreach (string file in Directory.EnumerateFiles(lifecamPath, "*.jpg"))
//            {
//                string fullPath = Path.Combine(lifecamPath, file);
//                using (var fileStream = new FileStream(fullPath, FileMode.OpenOrCreate))
//                {
//                    fileStream.Seek(0, SeekOrigin.Begin);
//                    AddFaceToList(fileStream, file, "");
//                }
//            }
//        }

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

                
        }

        //Creates a face list at the servers from microsoft
        private void CreateFaceList()
        {
            bool faceListExists = false;
            FaceList tmpFaceList;
            try
            {
                tmpFaceList = Task.Run(() => faceServiceClient.GetFaceListAsync(TMP_FACE_LIST.ToString())).Result;
                faceListExists = tmpFaceList != null;
            }
            catch (Exception e)
            {
                faceListExists = false;
            }
            if (faceListExists == false)
            {
                Task.Run(() => faceServiceClient.CreateFaceListAsync(TMP_FACE_LIST.ToString(), TMP_FACE_LIST_NAME, null));
                //before getting it again from microsoft, it needs time at the server to process, else it crashes
                Thread.Sleep(4);
            }

            //if the face list is not in de DB, adds it
            tmpFaceList = Task.Run(() => faceServiceClient.GetFaceListAsync(TMP_FACE_LIST.ToString())).Result;
            if (tmpFaceList != null && database.GetFaceListById(tmpFaceList.FaceListId) == null)
            {
                database.InsertFaceList(tmpFaceList.FaceListId, tmpFaceList.Name);
            }
        }

        //no need to use persons and groups so far
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

        private void DeleteTmpFaceList()
        {
            Task.Run(() => faceServiceClient.DeleteFaceListAsync(TMP_FACE_LIST.ToString()));
        }
    }
}
