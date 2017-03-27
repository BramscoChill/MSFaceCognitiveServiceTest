using System;
using System.Collections.Generic;
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
        private static Guid BASE_GROUP_ID = new Guid("7DA0173D-DF74-4B73-A2F0-9880130A7068");
        private static Guid BRAM_PERSON_ID = new Guid("f679205d-64b4-445b-99cf-9a08f87fb509");
        //private static Guid BRAM_PERSON_ID = new Guid("C3A70A67-DB64-49FD-B064-B32AFA8CE278");
        private static string BRAM_PERSON_NAME = "BRAMMEL";
        private static string BASE_GROUP_NAME = "BASE_GROUP";

        public FaceHelper()
        {
            string subscriptionKey = Constants.FACE_SUBSCRIPTION_KEY;
            faceServiceClient = new FaceServiceClient(subscriptionKey);
        }

        public void CreateBram()
        {
            bool bramExists = false;
            //checks if bram exists
            try
            {
                Person bramPerson = Task.Run(() => faceServiceClient.GetPersonAsync(BASE_GROUP_ID.ToString(), BRAM_PERSON_ID)).Result;
                bramExists = bramPerson != null;
            }
            catch (Exception e)
            {
                bramExists = false;
            }

            if (bramExists == false)
            {
                CreateBaseGroup();

                //creates dem bram
                CreatePersonResult bramPersonCreationResult = Task.Run(() => faceServiceClient.CreatePersonAsync(BASE_GROUP_ID.ToString(), BRAM_PERSON_NAME, null)).Result;
                if (bramPersonCreationResult.PersonId == BRAM_PERSON_ID)
                {
                    var a1 = 1;
                }
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
    }
}
