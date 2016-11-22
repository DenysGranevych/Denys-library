db.getCollection('labelingv3prod_copyFortestRenameSemyAnswers')

.update(

    {
        "labels.0.valence": "semy_negative"
    },

    {
        $set: {
            "labels.0.valence": "semi_negative"
        }
    },

    {
        multi: true
    })