db.getCollection('labelingv3qa').find({
    "labels.0.project": "test"
})