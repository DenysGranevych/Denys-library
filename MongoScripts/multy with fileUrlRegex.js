db.getCollection('labelingv3qa')
    .aggregate([{
        $unwind: '$labels'
    }, {
        $group: {
            _id: "$_id",

            gender: {
                $push: "$labels.gender"
            },

            numOfSpeakers: {
                $push: "$labels.numOfSpeakers"
            },

            moodgroup45: {
                $push: "$labels.moodgroup45"
            },

            FileUrl: {
                "$first": "$urls"
            }
        }
    }, {
        $project: {
            gender: "$gender",
            genderSize: {
                $size: "$gender"
            },
            numOfSpeakers: "$numOfSpeakers",
            numOfSpeakersSize: {
                $size: "$numOfSpeakers"
            },
            moodgroup45: "$moodgroup45",
            moodgroup45Size: {
                $size: "$moodgroup45"
            },
            fileUrl: "$FileUrl"
        }
    }, {
        $match: {
            genderSize: {
                $gte: 1,
                $lte: 6
            },

            numOfSpeakersSize: {
                $gt: 1
            },

            fileUrl: {
                $regex: "https://bvclabeling0pavel.blob.core.windows.net/.*"
            }
        }
    }])