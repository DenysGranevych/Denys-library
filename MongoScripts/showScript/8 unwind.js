db.getCollection('labelingv3qa')

.aggregate([

    {

        $unwind: '$labels'

    },

    {
        $match: {
            "labels.project": "test2"
        }
    }

])
    //return computed results