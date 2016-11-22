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

                $first: "$urls"

            }



        }

    }

])

//$first, $push, will can $min, $max, $last, $addToSet(uniq push), 

//The $group stage has a limit of 100 megabytes of RAM.