{
    aggregate([{
        "$unwind": "$labels"
    }, {
        "$group": {
            "_id": "$_id",
            "FileUrl": {
                "$first": "$urls"
            },
            "audioQuality": {
                "$push": "$labels.audioQuality"
            },
            "sex": {
                "$push": "$labels.sex"
            }
        }
    }, {
        "$project": {
            "FileUrl": "$FileUrl",
            "audioQuality": "$audioQuality",
            "audioQualitySize": {
                "$size": "$audioQuality"
            },
            "sex": "$sex",
            "sexSize": {
                "$size": "$sex"
            }
        }
    }, {
        "$match": {
            "audioQualitySize": {
                "$gte": 1,
                "$lte": 5
            },
            "sexSize": {
                "$gte": 1,
                "$lte": 5
            }
        }
    }])
}