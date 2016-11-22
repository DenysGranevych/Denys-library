db.getCollection('labelingv3qa').find({
    "labels.project": "test2",
    "labels.numOfSpeakers": {$gt : "1"}
})
//condition(for any in array) for any element in array