db.getCollection('labelsV3')
.aggregate([
{ $match: {"labels.project": "Training project_208"}},
    {
 
        $unwind: '$labels'
 
    },
   { $match: {"labels.project": "Training project_208"}},
   //{ $match: {"labels.sex": "femaleSex"}},
    {
 
        $group: {
            _id: "$labels.project",
 
            guid: {
                $push: "$_id"
            },
 
            sex: {
                $push: "$labels.sex"
            },
           /*answers : {
               $push : { id: "$_id", sex: "$labels.sex" }
               }*/
               
        }
 
    }
 
])