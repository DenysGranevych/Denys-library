db.getCollection('labelingv3qa').find({
    "labels": {
        $elemMatch: {
            "project": "test2",
            "numOfSpeakers": {$gt : "1"},
            "lablers":  "user@ua.com"
        }
    }
})
//condition(for element) for any element in array
//$elemMatch in $elemMatch not allow