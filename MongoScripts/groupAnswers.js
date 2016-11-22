db.getCollection('mongoslicenewsInoxoftDenysTest')

.aggregate([

    {
        $unwind: '$labels'
    },

    {
        $group: {
            _id: "$_id",
            sex: {
                $push: "$labels.sex"
            },
            feature: {
                $push: "$labels.feature"
            },
            numOfSpeakers: {
                $push: "$labels.numOfSpeakers"
            }
        }
    },

    {

        $project: {

            sex: "$sex",

            sexSize: {
                $size: "$sex"
            },

            feature: "$feature",

            featureSize: {
                $size: "$feature"
            },

            numOfSpeakers: "$numOfSpeakers",

            numOfSpeakersSize: {
                $size: "$numOfSpeakers"
            }

        }

    },

    {
        $match:

        {
            sexSize: {
                $gte: 1,
                $lte: 6
            },
            featureSize: {
                $gt: 1
            }
        }

    }

])