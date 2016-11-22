db.getCollection('labelingv3qa').find({
    "_id": {
        $regex: /[A-Z]/
    }
})