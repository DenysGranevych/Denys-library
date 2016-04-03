//DBRef 
public DBRef Owner { get; set; }
post.Owner = new DBRef("User", userId); //First parameter is a mongoDB collection name and second is object id

//to json
var documents = SpeCollection.AsQueryable();
It can also be converted to Json object-

var json = Json(documents, JsonRequestBehavior.AllowGet);


var filter = Builders<Post>.Filter.AnyEq(x => x.Tags, "mongodb");

// This will NOT compile:
// var filter = Builders<Post>.Filter.Eq(x => x.Tags, "mongodb");