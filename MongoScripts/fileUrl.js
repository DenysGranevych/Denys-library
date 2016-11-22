db.getCollection('InoxoftDenysTestDeleteFile')

.aggregate([

    {
        $unwind: '$labels'
    },

    {
        $group: {

            _id: "$_id",

            anger: {
                $push: "$labels.anger"
            },

            fileUrl: {
                $first: "$urls"
            }

        }

    },

    {

        $project: {

            anger: "$anger",

            angerSize: {
                $size: "$anger"
            },

            fileUrl: "$fileUrl",

        }

    },

    {
        $match:

        {
            angerSize: {
                $gte: 1,
                $lte: 6
            }
        }

    }

])