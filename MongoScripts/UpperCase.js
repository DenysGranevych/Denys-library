db.getCollection('labelsV3').find({
    "_id": {
        $regex: /[A-Z]/
    }
})