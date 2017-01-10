import RestSharp;

public string Server = "https://api.chayns.net/v2.0";
private const string Secret = "Your Tapp Secret";
public const int MangerUacGroup = 1;

[HttpPost]
[Route("UserPermitted")]
public IHttpActionResult UserPermitted(int locationId, int tappId, int userId)
{
    try
    {
        //Test if the user exists in the Group
        dynamic user = IsUserInGroup(locationId, tappId, userId, MangerUacGroup);

        //null means not in the Group
        if (user == null)
        {
            return Unauthorized();
        }
        else
        {
            //Build user-model
            object userModel = new
            {
                userId=user.userId,
                firstName=user.firstName,
                lastName=user.lastName,
                name=user.firstName,
                gender=user.gender
            };
            return Ok(userModel);
        }

    }
    catch (Exception exception)
    {
        return InternalServerError(exception);
    }
}

public dynamic IsUserInGroup(int locationId, int tappId, int userId, int groupId)
{
    //Set up RestSharp
    RestClient restClient = new RestClient(Server);

    //Build the request
    RestRequest req = new RestRequest("/" + locationId + "/Uac/" + groupId + "/User/" + userId);
    req.Method = Method.GET;
    req.AddHeader("Content-Type", "application/json");
    req.AddHeader("Authorization", "Basic  " + Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToString(tappId) + ':' + Secret)));

    //Run the request
    IRestResponse resp = restClient.Execute(req);

    //Test response health
    if (resp.StatusCode == HttpStatusCode.OK)
    {
        //Parse data
        dynamic data = JObject.Parse(resp.Content);
        //Just return the user-model
        return data.data[0];
    }
    else if (resp.StatusCode == HttpStatusCode.NotFound)
    {
        //null means not in the group
        return null;
    }
    else
    {
        //Throw an error for any other status code
        throw new Exception("The request went wrong");
    }
}
