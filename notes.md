# to do

## ui
2. better sign presentation
    - not cluttered/ambiguous
    - relative position + avoid overlapping
4. basic details page

## interactions
1. interactive [museum object]
    1. response on tapping/aiming at [museum object]
1. image/object recognition

## debug
3. location simulation

## infrastructure
1. resetting AR World origin

# ideas

## infrastructure
- [things] => points & areas (geo-fences)
- hierarchy & function of [things]
- update location based on indoor beacons, image/object recognition, QR code, manual input

## specifications

_object_ **location descriptor**
    _object_ **geometry**
        _Vector3_ **position**
        _Vector3_ **local anchor**
        _Vector3_ **scale** / _Mesh_ **mesh**
    _object_ **representations**
        _int[]_ **room hierarchy** { floor, room, ... }
        _int[]_ **beacons** { beacon id, angle, distance, ... } { get }
