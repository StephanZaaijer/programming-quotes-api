const mongodb = require('mongodb')
const mongoUri = require('../config/constants.js').mongoUri

const obrisiFilm = (req, res) => {
  mongodb.MongoClient.connect(mongoUri, (err, db) => {
    db.collection('filmovi').deleteOne({'_id': new mongodb.ObjectID(req.params.id)})
    res.send(`Unos sa ID ${req.params.id} je obrisan.`)
  })
}

module.exports = obrisiFilm