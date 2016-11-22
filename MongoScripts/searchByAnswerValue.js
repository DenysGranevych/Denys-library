db.getCollection('labelingv3prod_copyFortestRenameSemyAnswers')

.aggregate([

    {
        $unwind: '$labels'
    },

    {
        $group: {
            _id: "$_id",
            valence: {
                $push: "$labels.valence"
            }
        }
    },

    {

        $project: {

            valence: "$valence",

            valenceSize: {
                $size: "$valence"
            },

        }

    },

    {
        $match:

        {
            valenceSize: {
                $gte: 1,
                $lte: 6
            },
            valence: "semy_negative"
        },

    }

])