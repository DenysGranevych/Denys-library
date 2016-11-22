db.getCollection('InoxoftSliceContainerDenys2')

.find()

.forEach(function(doc) {

    //print( doc.urls[0] );  

    print(doc);

});


db.getCollection('labelingv3qa')

.aggregate([

        {

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
        }


    ])
    //return computed results