db.getCollection('mongoslicenewsInoxoftDenysTest').aggregate([{
    $unwind: '$labels'
}, {
    $project: {
        labels: 1,
        count: {
            $add: ["$"]
        }
    }
}, {
    $group: {
        _id: "$_id",
        number: {
            $sum: "$count"
        }
    }
}])