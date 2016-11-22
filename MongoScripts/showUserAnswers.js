db.getCollection('labelingv3prod')

.aggregate([

    {
        $unwind: '$labels'
    },

    {
        $group: {
            _id: "$labels.lablers",
            valence: {
                $push: "$labels.task"
            }
        }
    },

    {

        $project: {

            valence: "$valence"

        }

    },



])