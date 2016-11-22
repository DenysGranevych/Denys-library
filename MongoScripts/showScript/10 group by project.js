db.getCollection('labelingv3qa')
.aggregate([

    {

        $unwind: '$labels'

    }, {

        $group: {
            _id: "$labels.project",
             users: {

                $push: "$labels.lablers"

            },

        }

    }

])

//$first, $push, will can $min, $max, $last, $addToSet(uniq push), 

//The $group stage has a limit of 100 megabytes of RAM.